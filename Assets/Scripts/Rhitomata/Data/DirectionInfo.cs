using UnityEngine;
using System.Collections.Generic;

namespace Rhitomata.Data {
    public class DirectionInfo {
        public float time = 0;
        // This is supposed to be rotation but we called it directions anyway lol
        public List<Vector3> directions = new() {
            new (0, 0, 0),
            new (0, 0, -90)
        };
    }
}