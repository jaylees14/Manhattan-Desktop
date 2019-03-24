using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Controller;

namespace Model.BlueprintUI {
    public class GoalResourceUI : IBlueprintUIMode {
        public BlueprintUIController blueprintUIController { get; set; }
        public Canvas BlueprintUICanvas { get; set; }
        public GameObject Title { get; set; }
        public String TitleStr { get; set; }
        public List<GameObject> CanvasObjects { get; set; }
        public ManhattanAnimation animationManager { get; set; }

        void IBlueprintUIMode.OnInitialize() {
        }

        void IBlueprintUIMode.OnShow() {
            BlueprintUITools.NewGoal(BlueprintUICanvas.transform, CanvasObjects, 0.5f, 0.4f, 0.25f, 32, 28, 30, 31);

            BlueprintUITools.CreateInfoText(BlueprintUICanvas.transform, CanvasObjects,
                "Craft this to win! ᕙ(⇀‸↼‶)ᕗ");
        }

        void IBlueprintUIMode.OnHide() {
        }
    }
}
