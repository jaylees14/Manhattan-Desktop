using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Model.BlueprintUI {
    // Abstraction tools for creating the Blueprint UI
    public static class BlueprintUITools {

        ////////////////////////////////////////////////////////////////////////
        // Creating Unity containers
        ////////////////////////////////////////////////////////////////////////
        // Create a new GameObject for blueprint UI use.
        public static GameObject NewObject(Transform parent, List<GameObject> objList, Vector2 position) {
            GameObject obj = new GameObject();
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.AddComponent<RectTransform>();
            obj.AddComponent<CanvasRenderer>();
            objList.Add(obj);
            return obj;
        }

        // Create a new 2D ui item based off a texture.
        public static GameObject New2D(Transform parent, List<GameObject> objList, Vector2 position, float relativeSize, Texture texture) {
            GameObject obj2D = NewObject(parent, objList, position);
            RawImage obj2DImage = obj2D.AddComponent<RawImage>();
            obj2DImage.texture = texture;
            (obj2D.transform as RectTransform).sizeDelta = new Vector2(relativeSize, relativeSize);
            (obj2D.transform as RectTransform).localScale = new Vector3(1.0f, 1.0f, 0.0f);
            return obj2D;
        }

        // Create a new 2D ui item based off a sprite.
        public static GameObject New2D(Transform parent, List<GameObject> objList, Vector2 position, float relativeSize, Sprite sprite) {
            GameObject obj2D = NewObject(parent, objList, position);
            SVGImage obj2DImage = obj2D.AddComponent<SVGImage>();
            obj2DImage.sprite = sprite;
            (obj2D.transform as RectTransform).sizeDelta = new Vector2(relativeSize, relativeSize);
            (obj2D.transform as RectTransform).localScale = new Vector3(1.0f, 1.0f, 0.0f);
            return obj2D;
        }

        // Create an overlay with number of resources out of total required.
        public static GameObject NewInfoNumber(Transform parent, List<GameObject> objList, Vector2 position, float relativeSize, int id, int required) {
            string text = "/" + required.ToString();

            // TODO: Make equal to num in inventory
            if (required == 0) text = "";

            GameObject num = new GameObject();
            num.transform.SetParent(parent);
            num.AddComponent<RectTransform>();
            num.AddComponent<CanvasRenderer>();
            Text numText = num.AddComponent<Text>();
            numText.text = text;
            numText.alignment = TextAnchor.MiddleCenter;
            numText.fontSize = (int)(relativeSize / 4.0f);
            numText.font = Resources.Load<Font>("Fonts/HelveticaNeueMedium");
            numText.color = new Color(9.0f, 38.0f, 66.0f);
            num.transform.position = position;
            (num.transform as RectTransform).sizeDelta = new Vector2(relativeSize, relativeSize);
            objList.Add(num);
            return num;
        }

        // Create a new button that is also an svg image.
        public static Button CreateButton(Transform parent, List<GameObject> objList, Vector2 position, float scale, Sprite sprite) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            Vector2 relativePosition = sp.ToV(position);
            float relativeSize = sp.ToH(scale);

            GameObject obj = New2D(parent, objList, position, relativeSize, sprite);

            Button btn = obj.AddComponent<Button>();
            btn.transform.position = relativePosition;
            ColorBlock colors = btn.colors;
            // colors.normalColor = new Color(255f, 255f, 255f, 0.1f);
            colors.highlightedColor = new Color(235f, 91f, 92f, 0.5f);
            // colors.pressedColor = new Color(122f, 122f, 122f);
            btn.colors = colors;

            return btn;
        }

        // Create a new button that is also an svg image.
        public static Button CreateButton(Transform parent, List<GameObject> objList, Vector2 position, float scale, Texture texture) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            Vector2 relativePosition = sp.ToV(position);
            float relativeSize = sp.ToH(scale);

            GameObject obj = New2D(parent, objList, position, relativeSize, texture);

            Button btn = obj.AddComponent<Button>();
            btn.transform.position = relativePosition;
            ColorBlock colors = btn.colors;
            // colors.normalColor = new Color(255f, 255f, 255f, 0.1f);
            colors.highlightedColor = new Color(235f, 91f, 92f, 0.5f);
            // colors.pressedColor = new Color(122f, 122f, 122f);
            btn.colors = colors;

            return btn;
        }

        ////////////////////////////////////////////////////////////////////////
        // Less abstract resource drawing.
        ////////////////////////////////////////////////////////////////////////
        // Create a dark border around a hexagon.
        public static void NewMachineBorder(Transform parent, List<GameObject> objList, float x, float y, float scale) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            Vector2 position = sp.ToV(new Vector2(x, y));
            float relativeSize = sp.ToH(scale);

            New2D(parent, objList, position, relativeSize * 1.1f, Resources.Load<Sprite>("slot_border_outer"));
        }

        // Create a dark border around a hexagon.
        public static void NewCraftableBorder(Transform parent, List<GameObject> objList, float x, float y, float scale) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            Vector2 position = sp.ToV(new Vector2(x, y));
            float relativeSize = sp.ToH(scale);

            New2D(parent, objList, position, relativeSize * 1.1f, Resources.Load<Sprite>("slot_border_dark"));
        }

        // Create a dark border around a hexagon.
        public static void NewAvailableBorder(Transform parent, List<GameObject> objList, float x, float y, float scale) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            Vector2 position = sp.ToV(new Vector2(x, y));
            float relativeSize = sp.ToH(scale);

            New2D(parent, objList, position, relativeSize * 1.1f, Resources.Load<Sprite>("slot_border_highlight"));
        }

        // Create a basic arrow.
        public static void NewCraftArrow(Transform parent, List<GameObject> objList, float x, float y, float scale) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            Vector2 position = sp.ToV(new Vector2(x, y));
            float relativeSize = sp.ToH(scale);

            New2D(parent, objList, position, relativeSize * 0.7f, Resources.Load<Texture>("CraftArrow"));
        }

        // Create a new resource shown in a hex tile.
        public static void NewResource(Transform parent, List<GameObject> objList, float x, float y, float scale, int id, int required=0) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            Vector2 position = sp.ToV(new Vector2(x, y));
            float relativeSize = sp.ToH(scale);

            New2D(parent, objList, position, relativeSize, Resources.Load<Sprite>("inventory_slot"));
            New2D(parent, objList, position, relativeSize * 0.7f, AssetManager.Instance().GetItemTexture(id, "Models/2D/"));
            NewInfoNumber(parent, objList, position, relativeSize, id, required);
        }

        ////////////////////////////////////////////////////////////////////////
        // Effects of crafting an item
        ////////////////////////////////////////////////////////////////////////
        public static Boolean ViableCraft() {
            // Return true if inventory quantities are large enough
            return true;
        }

        public static void ProcessCraft() {

        }

        ////////////////////////////////////////////////////////////////////////
        // UI specific creation of recipes
        ////////////////////////////////////////////////////////////////////////
        // Create a new visual craftable instruction.
        public static void NewCraftable(Transform parent, List<GameObject> objList,
                                        float x, float y, float scale, int resultID,
                                        int resourceIDA=0, int resourceIDARequired=0,
                                        int resourceIDB=0, int resourceIDBRequired=0,
                                        int resourceIDC=0, int resourceIDCRequired=0) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            float relativeSize = sp.ToH(scale);
            float niceRatio = 0.75f; // Slightly less than sqrt(3)/2. Hexagon stuff.

            NewCraftArrow(parent, objList, x + scale * 0.6f, y, scale);
            NewResource(parent, objList, x, y, scale, resourceIDA, resourceIDARequired);
            if (resourceIDB > 0)
                NewResource(parent, objList, x - scale * 0.6f, y, scale, resourceIDB, resourceIDBRequired);
            if (resourceIDC > 0)
                NewResource(parent, objList, x - scale * 0.3f, y + scale * niceRatio, scale, resourceIDC, resourceIDCRequired);

            New2D(parent, objList, sp.ToV(new Vector2(x + scale * 1.2f, y)),
                relativeSize, Resources.Load<Sprite>("inventory_slot"));
            Button createButton = CreateButton(parent, objList, new Vector2(x + scale * 1.2f, y), scale * 0.7f, Resources.Load<Texture>("Models/2D/sprite_" + resultID.ToString()));
            NewCraftableBorder(parent, objList, x + scale * 1.2f, y, scale);
        }

        // Create a new visual craftable instruction.
        public static void NewMachine(Transform parent, List<GameObject> objList, float x, float y, float scale, int resultID, int machineID, int resourceIDA, int resourceIDB=0) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            float relativeSize = sp.ToH(scale);
            float niceRatio = 0.8f; // Slightly less than sqrt(3)/2. Hexagon stuff.

            NewResource(parent, objList, x + scale * 1.2f, y, scale, resultID);
            NewCraftArrow(parent, objList, x + scale * 0.6f, y, scale);
            NewResource(parent, objList, x - scale * 0.6f, y, scale, resourceIDA);
            if (resourceIDB > 0)
                NewResource(parent, objList, x - scale * 0.3f, y + scale * niceRatio, scale, resourceIDB);
            NewResource(parent, objList, x, y, scale, machineID);
            NewMachineBorder(parent, objList, x, y, scale);
        }

        // Create a new visual craftable blueprint.
        public static void NewBlueprint(Transform parent, List<GameObject> objList, float x, float y, float scale, int resultID, int resourceIDA, int resourceIDB=0, int resourceIDC=0) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            float relativeSize = sp.ToH(scale);
            float niceRatio = 0.8f; // Slightly less than sqrt(3)/2. Hexagon stuff.

            NewCraftArrow(parent, objList, x + scale * 0.6f, y, scale);
            NewResource(parent, objList, x - scale * 0.6f, y, scale, resourceIDA);
            if (resourceIDB > 0)
                NewResource(parent, objList, x - scale * 0.3f, y + scale * niceRatio, scale, resourceIDB);
            if (resourceIDC > 0)
                NewResource(parent, objList, x - scale * 0.3f, y + scale * niceRatio, scale, resourceIDC);

            NewResource(parent, objList, x + scale * 1.2f, y, scale, resultID);
            NewCraftableBorder(parent, objList, x + scale * 1.2f, y, scale);
        }

        // Create the visual goal to win.
        public static void NewGoal(Transform parent, List<GameObject> objList, float x, float y, float scale, int resultID, int resourceIDA=0, int resourceIDB=0, int resourceIDC=0) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();
            float relativeSize = sp.ToH(scale);
            float niceRatio = 0.8f; // Slightly less than sqrt(3)/2. Hexagon stuff.

            if (resourceIDA > 0)
                NewResource(parent, objList, x - scale * 0.6f, y, scale, resourceIDA);
            if (resourceIDB > 0)
                NewResource(parent, objList, x + scale * 0.3f, y + scale * niceRatio, scale, resourceIDB);
            if (resourceIDC > 0)
                NewResource(parent, objList, x + scale * 0.3f, y - scale * niceRatio, scale, resourceIDC);

            NewResource(parent, objList, x, y, scale, resultID);
            NewMachineBorder(parent, objList, x, y, scale);
        }

        // Create the title used for each blueprint UI menu.
        public static GameObject CreateTitle(Transform parent, List<GameObject> objList, string titleText) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();

            // TODO: Fonts not loading currently for some reason.
            GameObject title = new GameObject();
            title.transform.SetParent(parent);
            title.AddComponent<RectTransform>();
            title.AddComponent<CanvasRenderer>();
            Text newText = title.AddComponent<Text>();
            newText.text = titleText;
            newText.alignment = TextAnchor.MiddleCenter;
            newText.fontSize = 80;
            newText.font = Resources.Load<Font>("Fonts/MeckaBleckaRegular");
            newText.color = new Color(9.0f, 38.0f, 66.0f);
            title.transform.position = sp.ToV(new Vector2(0.5f, 0.9f));
            (title.transform as RectTransform).sizeDelta = new Vector2(sp.ToH(1.0f), sp.ToH(0.2f));
            objList.Add(title);
            return title;
        }

        // Create infomative text for each blueprint UI menu.
        public static GameObject CreateInfoText(Transform parent, List<GameObject> objList, string infoText) {
            ScreenProportions sp = GameObject.Find("ScreenProportions").GetComponent<ScreenProportions>();

            GameObject title = new GameObject();
            title.transform.SetParent(parent);
            title.AddComponent<RectTransform>();
            title.AddComponent<CanvasRenderer>();
            Text newText = title.AddComponent<Text>();
            newText.text = infoText;
            newText.alignment = TextAnchor.MiddleCenter;
            newText.fontSize = 20;
            newText.font = Resources.Load<Font>("Fonts/HelveticaNeueMedium");
            newText.color = new Color(9.0f, 38.0f, 66.0f);
            title.transform.position = sp.ToV(new Vector2(0.5f, 0.85f));
            (title.transform as RectTransform).sizeDelta = new Vector2(sp.ToH(1.0f), sp.ToH(0.2f));
            objList.Add(title);
            return title;
        }
    }
}
