using NSSLC.Infrastructure.Network;
using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal static class PlayerSlotSessionAuthorityVerification {
  public static async Task RunAsync() {
    var authority = new PlayerSlotSessionAuthority(playerSlotCount: 2, password: "secret");
    var firstConnection = new ConnectionIdentity(Guid.NewGuid(), 1);
    var secondConnection = new ConnectionIdentity(Guid.NewGuid(), 1);
    var thirdConnection = new ConnectionIdentity(Guid.NewGuid(), 1);

    SessionAdmission missingPassword = await authority.AdmitAsync(firstConnection, null,
        CancellationToken.None);
    SessionAdmission incorrectPassword = await authority.AdmitAsync(firstConnection, "wrong",
        CancellationToken.None);
    Verify.That(missingPassword.Binding is null
        && missingPassword.RejectionCode == "AuthenticationRejected"
        && incorrectPassword.Binding is null
        && incorrectPassword.RejectionCode == "AuthenticationRejected"
        && authority.ActiveBindings == 0,
        "Rejected credentials must not allocate a production player slot.");

    SessionAdmission first = await authority.AdmitAsync(firstConnection, "secret",
        CancellationToken.None);
    SenderBinding firstBinding = first.Binding
        ?? throw new InvalidOperationException("Valid credentials must allocate a player binding.");
    Verify.That(firstBinding.PlayerSlot == 0
        && firstBinding.GameSessionKey == authority.GameSessionKey
        && authority.AdmissionCount == 1,
        "A valid password must bind the first available slot to the running game session.");

    SessionAdmission duplicate = await authority.AdmitAsync(firstConnection, "secret",
        CancellationToken.None);
    Verify.That(duplicate.Binding is null && duplicate.RejectionCode == "DuplicateConnection",
        "A connection must not receive multiple player bindings.");

    SessionAdmission second = await authority.AdmitAsync(secondConnection, "secret",
        CancellationToken.None);
    SenderBinding secondBinding = second.Binding
        ?? throw new InvalidOperationException("The second player must receive a binding.");
    Verify.That(secondBinding.PlayerSlot == 1
        && secondBinding.GameSessionKey == authority.GameSessionKey,
        "Separate admitted players must receive distinct slots in the same game session.");
    var mismatchedBinding = new SenderBinding(0, secondBinding.GameSessionKey);
    await Verify.ThrowsAsync<InvalidOperationException>(() =>
        authority.ReleaseAsync(secondConnection, mismatchedBinding, CancellationToken.None).AsTask());
    Verify.That(authority.ActiveBindings == 2,
        "A mismatched release must not free a binding owned by another player slot.");
    SessionAdmission full = await authority.AdmitAsync(thirdConnection, "secret",
        CancellationToken.None);
    Verify.That(full.Binding is null && full.RejectionCode == "ServerFull"
        && authority.ActiveBindings == 2,
        "A full server must reject admission without changing existing bindings.");

    await authority.ReleaseAsync(firstConnection, firstBinding, CancellationToken.None);
    Verify.That(authority.ActiveBindings == 1,
        "Disconnect cleanup must release the exact current player binding.");
    SessionAdmission recycled = await authority.AdmitAsync(thirdConnection, "secret",
        CancellationToken.None);
    SenderBinding recycledBinding = recycled.Binding
        ?? throw new InvalidOperationException("The released slot must be reusable.");
    Verify.That(recycledBinding.PlayerSlot == 0
        && recycledBinding.GameSessionKey == authority.GameSessionKey,
        "A released slot must return to the available pool for a later connection.");

    await Verify.ThrowsAsync<InvalidOperationException>(() =>
        authority.ReleaseAsync(firstConnection, firstBinding, CancellationToken.None).AsTask());
    Verify.That(authority.ActiveBindings == 2,
        "A stale disconnect must not release the recycled slot's current binding.");
    using var canceled = new CancellationTokenSource();
    canceled.Cancel();
    await Verify.ThrowsAsync<OperationCanceledException>(() =>
        authority.AdmitAsync(new ConnectionIdentity(Guid.NewGuid(), 1), "secret",
            canceled.Token).AsTask());

    var openAuthority = new PlayerSlotSessionAuthority(playerSlotCount: 1);
    SessionAdmission openAdmission = await openAuthority.AdmitAsync(
        new ConnectionIdentity(Guid.NewGuid(), 1), null, CancellationToken.None);
    Verify.That(!openAuthority.RequiresPassword && openAdmission.Binding is { PlayerSlot: 0 },
        "A host without a configured password must admit clients into its bounded slot pool.");

    var hostAuthority = new PlayerSlotSessionAuthority(playerSlotCount: 1,
        hostToken: "trusted-host");
    var hostConnection = new ConnectionIdentity(Guid.NewGuid(), 1);
    SessionAdmission hostAdmission = await hostAuthority.AdmitAsync(hostConnection, null,
        CancellationToken.None);
    SenderBinding hostBinding = hostAdmission.Binding
        ?? throw new InvalidOperationException("Host authorization requires an admitted actor.");
    var hostContext = new NetworkSessionContext(hostConnection, "test-profile",
        NetworkSessionStage.AwaitPlayerData, hostBinding, false);
    Verify.That(hostAuthority.ClientUuids.TryRecord(hostConnection, "client-uuid-68"),
        "The authority-owned UUID registry must accept the current connection epoch.");
    Verify.That(await hostAuthority.AuthorizeHostAsync(hostContext, "trusted-host",
            CancellationToken.None)
        && !await hostAuthority.AuthorizeHostAsync(hostContext, "wrong-host",
            CancellationToken.None)
        && !await hostAuthority.AuthorizeHostAsync(hostContext, string.Empty,
            CancellationToken.None),
        "Configured host tokens must use exact constant-time matching and reject empty or wrong values.");
    Verify.That(!await hostAuthority.AuthorizeHostAsync(
            hostContext with { Connection = new ConnectionIdentity(Guid.NewGuid(), 1) },
            "trusted-host", CancellationToken.None)
        && !await hostAuthority.AuthorizeHostAsync(
            hostContext with { Actor = new SenderBinding(1, hostAuthority.GameSessionKey) },
            "trusted-host", CancellationToken.None),
        "A host token must not authorize a forged connection identity or sender actor.");
    await hostAuthority.ReleaseAsync(hostConnection, hostBinding, CancellationToken.None);
    Verify.That(!await hostAuthority.AuthorizeHostAsync(hostContext, "trusted-host",
            CancellationToken.None),
        "A released binding must invalidate every context captured before disconnect.");
    Verify.That(!hostAuthority.ClientUuids.TryGet(hostConnection, out _),
        "Releasing a binding must remove the UUID associated with the old connection epoch.");
    var unconfiguredHost = new PlayerSlotSessionAuthority(playerSlotCount: 1);
    SessionAdmission unconfiguredAdmission = await unconfiguredHost.AdmitAsync(
        new ConnectionIdentity(Guid.NewGuid(), 1), null, CancellationToken.None);
    SenderBinding unconfiguredBinding = unconfiguredAdmission.Binding
        ?? throw new InvalidOperationException("The unconfigured host fixture must admit a player.");
    Verify.That(!await unconfiguredHost.AuthorizeHostAsync(
            new NetworkSessionContext(new ConnectionIdentity(Guid.NewGuid(), 1), "test-profile",
                NetworkSessionStage.AwaitPlayerData, unconfiguredBinding, false),
            "trusted-host", CancellationToken.None),
        "An omitted host token must never grant host authority.");
  }
}
