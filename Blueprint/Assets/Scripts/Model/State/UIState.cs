using System.Threading;
using UnityEngine;

namespace Model.State {
    public class UIState {
        public enum OpenUI {
            Welcome,
            Login,
            Playing,
            Inventory,
            Blueprint,
            Machine,
            Goal,
            Pause,
            Exit,
            Logout,
            Bindings,
            Gate,
            Mouse,
        };

        public OpenUI Selected;
        public Vector2 SelectedMachineLocation;

        public UIState() {
            this.Selected = OpenUI.Login;
        }
    }
}
