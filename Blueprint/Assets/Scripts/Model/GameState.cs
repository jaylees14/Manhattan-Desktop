using System;
using Model.State;
using UnityEngine;

/* Wrapper for states to transfer to/from the server */
namespace Model {
    [Serializable]
    public class GameState {
        public MapState mapState;
        public HeldItemState heldItemState;
        public InventoryState inventoryState;
        public MachineState machineState;

        public GameState(MapState mapState, HeldItemState heldItemState, InventoryState inventoryState, MachineState machineState) {
            this.mapState = mapState;
            this.heldItemState = heldItemState;
            this.inventoryState = inventoryState;
            this.machineState = machineState;
        }
    }
}