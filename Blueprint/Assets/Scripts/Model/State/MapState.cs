using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Model.State {
    [Serializable]
    public class MapState {
        [SerializeField] private Dictionary<Vector2, MapObject> grid;
        [SerializeField] private Goal goal;
        [SerializeField] private List<WirePath> wirePaths;
        [SerializeField] private bool IntroComplete;

        public MapState() {
            grid = new Dictionary<Vector2, MapObject>();
            goal = new Goal();
            wirePaths = new List<WirePath>();
        }

        public void AddObject(Vector2 position, int id, int rotation) {
            grid[position] = new MapObject(id, rotation);
        }

        public void RemoveObject(Vector2 position) {
            grid.Remove(position);
        }

        public void RotateObject(Vector2 position) {
            grid[position].Rotate();
        }

        public void addGoalItem(GoalPosition position) {
            if (position == GoalPosition.Top)
                goal.topInput = true;
            if (position == GoalPosition.Mid)
                goal.midInput = true;
            if (position == GoalPosition.Bot)
                goal.botInput = true;
        }

        public Dictionary<Vector2, MapObject> getObjects() {
            return grid;
        }

        public Goal getGoal() {
            return goal;
        }
        
        public Dictionary<Vector2, MapObject> GetObjects() {
            return grid;
        }

        public void AddWirePath(WirePath path) {
            wirePaths.Add(path);
        }

        public void RemoveWirePaths(Vector2 location) {
            this.wirePaths = wirePaths.FindAll(x => x.start != location && x.end != location);
        }

        public List<WirePath> GetWirePaths() {
            return wirePaths;
        }

        public bool GetIntroState() {
            return this.IntroComplete;
        }

        public void SetIntroState(bool state) {
            this.IntroComplete = state;
        }
    }
}
