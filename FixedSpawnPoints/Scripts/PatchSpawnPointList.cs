using System.Collections.Generic;
using System.Linq;
using RWG2;
using UnityEngine;

public class PatchSpawnPointList
{
  public static bool useFixed = true;
  public static Dictionary<Vector2, float> fixedSpawnList = new Dictionary<Vector2, float> {{new Vector2(0, 0), 0f}};

  public static SpawnPosition PatchGetRandomSpawnPosition(SpawnPointList list, Vector3? _refPosition, int _minDistance, int _maxDistance)
  {
    //if useFixed is true then use our fixed spawn point when no bedroll set
    if (useFixed)
    {
      //add logic to select from the list based on rnd
      var i = fixedSpawnList.First();
      var _x = i.Key.x;
      var _y = i.Key.y;

      var vector = new Vector3(_x, RWGCore.Instance.biomeProvider.GetFinalWorldHeightAt(_x, _y) + 1, _y);
      return new SpawnPosition(vector, i.Value);
    }

    //vanilla spawn selection
    if (list.Count == 0) return SpawnPosition.Undef;

    if (!_refPosition.HasValue) return list[Random.Range(0, list.Count)].spawnPosition;

    //try 50 times to find a spawn within the range given (500 - 2500 for backpack on death)
    for (var i = 0; i < 50; i++)
    {
      var spawnPosition = list[Random.Range(0, list.Count)].spawnPosition;
      var magnitude = (spawnPosition.position - _refPosition.Value).magnitude;

      if (magnitude >= _minDistance && magnitude <= _maxDistance) return spawnPosition;
    }

    //couldnt find somewhere in range so use full list to search for nearest
    var num = 3.40282347E+38f;
    var num2 = -1;
    for (var j = 0; j < list.Count; j++)
    {
      var spawnPosition2 = list[j].spawnPosition;
      var magnitude2 = (spawnPosition2.position - _refPosition.Value).magnitude;
      if (!(magnitude2 < num)) continue;

      num2 = j;
      //added to get closest spawn to _refPosition instead of last item in list
      num = magnitude2;
    }

    return num2 != -1 ? list[num2].spawnPosition : SpawnPosition.Undef;
  }
}
