# NPCInteractions Qualification Boundary

Source oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria.GameContent\NPCInteractions.cs`

`NPCInteractions.Initialize` registers 25 shop entries (NPC type/shop index pairs, including the
Painter custom text entry) and 18 named actions. The action families include shop opening,
sign/chat, nurse healing, tax collection, Dryad purification, Angler quests, housing, crafting,
reforging, stylist UI and happiness reporting.

## Qualification result

This family is not eligible for a single Simulation definition child. Registration identity alone
does not provide an authoritative contract: conditions and effects depend on player state, chat/UI,
shop/economy, quest, housing, crafting and presentation owners. The source artifact also does not
provide a bounded server predicate for those actions in this migration slice.

Creating an ID/name catalog would therefore be non-authoritative and would falsely imply that the
interaction lifecycle had been migrated.

## Decision

Keep `NPCInteractions.Initialize` `unknown/deferred`. Split future work by owner: shop inventory,
typed player interaction commands, quest state, housing, crafting/reforge and presentation/chat.
Do not add a generic `NpcInteractionRegistry` or aggregate initializer.
