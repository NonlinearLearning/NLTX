using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Network;

public sealed class CommunicationSessionCommitSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly CommunicationIngressPreparationSystem _preparation;

  public CommunicationSessionCommitSystem(ArchWorld world, CommunicationIngressPreparationSystem preparation) : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _preparation = preparation ?? throw new ArgumentNullException(nameof(preparation));
  }

  public override void Update(in EcsServerTickContext context)
  {
    _preparation.CommitPendingClosures(context.CommandBuffer);
  }
}
