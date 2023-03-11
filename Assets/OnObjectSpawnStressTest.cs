using System;
using System.Diagnostics;
using Plugins.ObjectPooler;
using Unity.VisualScripting;
using UnityEngine;

public class OnObjectSpawnStressTest : MonoBehaviour
{
   [Header("References")]
   [SerializeField] private ObjectPooler _objectPooler;

   [Header("Preferences")]
   [SerializeField] private int _size = 5000;
   [SerializeField] private Pool _pool = Pool.TestPool2;
   
   private void OnValidate()
   {
      _objectPooler ??= FindObjectOfType<ObjectPooler>();
   }

   [ContextMenu("Start Test")]
   private void Test()
   {
      Stopwatch stopwatch = new Stopwatch();

      for (int i = 0; i < _size; i++)
      {
         stopwatch = Stopwatch.StartNew();
         _objectPooler.Spawn(_pool);
         stopwatch.Stop();
         UnityEngine.Debug.Log("Spawn Time: " + stopwatch.Elapsed.Ticks);
      }
   }
}
