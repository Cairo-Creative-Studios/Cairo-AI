using UDT.Core;

namespace AI.Core
{
    public class BrainData : ComponentData<Brain>
    {
        
    }
    
    public class BrainData<T> : BrainData where T : Brain
    {
    }
}