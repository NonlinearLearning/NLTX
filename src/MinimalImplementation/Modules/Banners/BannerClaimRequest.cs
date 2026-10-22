namespace MinimalImplementation.Server.Modules.Banners;

public sealed record BannerClaimRequest(short BannerId, ushort Amount);

public sealed record BannerClaimResponse(short BannerId, ushort Amount, bool Granted);

public sealed record BannerClaimRequestPayload(short BannerId, ushort Amount);
