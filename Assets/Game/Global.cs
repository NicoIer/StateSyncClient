using UnityToolkit;

namespace Game
{
    public class Global : MonoSingleton<Global>
    {
        protected override bool DontDestroyOnLoad() => true;
    }
}