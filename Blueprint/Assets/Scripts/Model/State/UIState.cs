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
            InvPause,
            BluePause,
            MachPause,
            Exit,
            InvExit,
            BlueExit,
            MachExit,
            Logout,
            InvLogout,
            BlueLogout,
            MachLogout,
            Bindings,
        };

        public OpenUI Selected;
        public Vector2 SelectedMachineLocation;

        public UIState() {
            this.Selected = OpenUI.Login;
        }
    }
}
