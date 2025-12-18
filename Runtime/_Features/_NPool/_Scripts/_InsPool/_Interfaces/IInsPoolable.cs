using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    public interface IInsPoolable
    {
        void onCreated(bool isRoot) { }
        void onSpawn(bool isRoot) { }
        void onDespawn(bool isRoot) { }
        void onDestroy(bool isRoot) { }
    }
}