using Terraria.Relationships;
using Terraria.Entity;

namespace Terraria.Communication;

public interface ICommunicationSessionPlayerLifecycle
{
  EntityRuntimeHandle CreatePlayer(SessionEpoch epoch, EcsServerCommandBuffer commands);

  void DestroyPlayer(
      SessionEpoch epoch,
      EntityRuntimeHandle player,
      EcsServerCommandBuffer commands);
}
