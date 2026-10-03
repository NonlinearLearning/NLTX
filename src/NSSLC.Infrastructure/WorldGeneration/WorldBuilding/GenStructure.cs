using NSSLC.WorldGeneration.Geometry;

namespace NSSLC.WorldGeneration.WorldBuilding;

public abstract class GenStructure : GenBase
{
	public virtual bool Place(Point origin, StructureMap structures)
{
	
		return Place(origin, structures, null);
	
	}
	public abstract bool Place(Point origin, StructureMap structures, GenerationProgress progress);
}
