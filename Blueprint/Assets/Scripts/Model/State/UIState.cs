using UnityEngine;

namespace Model.State {
    public class UIState {
        public enum OpenUI {
            Welcome,
            Login,
            Playing,
            Inventory,
            Blueprint,
            BlueprintTemplate,
            Machine,
            Goal,
            Pause,
            Exit,
            Logout,
            BindingsPause,
            BindingsIntro,
            Gate,
            Mouse,
            Intro,
        };

        public OpenUI Selected;
        public Vector2 SelectedMachineLocation;
        public int SelectedBlueprintID = -1;

        public UIState() {
            this.Selected = OpenUI.Login;
        }
    }
}
