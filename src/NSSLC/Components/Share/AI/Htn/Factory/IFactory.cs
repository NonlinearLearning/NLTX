// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable


using System.Collections.Generic;

namespace Terraria.Ai.Htn.Factory
{
    public interface IFactory
    {
        T[] CreateArray<T>(int length);
        bool FreeArray<T>(ref T[] array);

        Queue<T> CreateQueue<T>();
        bool FreeQueue<T>(ref Queue<T> queue);

        List<T> CreateList<T>();
        bool FreeList<T>(ref List<T> list);

        T Create<T>() where T : new();
        bool Free<T>(ref T obj);
    }
}
