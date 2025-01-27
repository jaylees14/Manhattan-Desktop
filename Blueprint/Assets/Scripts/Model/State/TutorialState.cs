using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Model.State {
    public class TutorialState {
        public enum TutorialStage {
            Welcome, 
            Moving,
            ShouldOpenInventory,
            InsideInventory,
            DidMoveInventoryItem,
            DidSplitInventoryItem,
            DidCollectFromBackpack,
            ShouldCloseInventory,
            ClosedInventory,
            ShouldOpenBlueprint,
            InsideBlueprint,
            HighlightFurnace,
            OpenFurnace,
            CraftedFurnace,
            HighlightBlueprintNotes,
            ReturnToProgression,
            ShouldCloseBlueprintTemplate, 
            ShouldCloseBlueprint, 
            ClosedBlueprint,
            ShowHeldItemScroll,
            ScrolledHeldItem,
            PlacedFurnace,
            ShouldOpenMachine,
            ShowMachineOutputSlots,
            ShowAllMachine,
            DidCraftInMachine,
            DidMoveCraftToInventory
        }
        public TutorialStage stage;

        public TutorialState() {
            this.stage = TutorialStage.Welcome;
        }
    }
}