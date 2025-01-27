using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Model;
using Model.Action;
using Model.Reducer;
using Model.Redux;
using Model.State;
using UnityEditor;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

/* Attached to each slot in the inventory grid */
namespace Controller {
    public class InventorySlotController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        internal int id;
        private bool mouseOver;
        public Optional<InventoryItem> storedItem;
        private GameObject highlightObject;
        private float slotHeight;
        private float slotWidth;
        public float originalSlotHeight;
        private GameManager gameManager;
        private AssetManager assetManager;
        private float mouseEntryTime;
        private GameObject rolloverObject;
        private Vector3 rolloverPosition;
        private bool rolloverState;
        private Text rolloverObjectText;

        // EDITABLE
        // Time before rollover text shows (secs)
        private float rolloverTime = 1.0f;

        private void Start() {
            highlightObject = GameObject.Find(this.transform.parent.name + "/Highlight");
            rolloverObject = GameObject.Find(this.transform.parent.name + "/Rollover");
            slotHeight = (transform as RectTransform).rect.height;
            slotWidth = (transform as RectTransform).rect.width;
            originalSlotHeight = slotHeight;
            storedItem = Optional<InventoryItem>.Empty();
            gameManager = GameManager.Instance();
            assetManager = AssetManager.Instance();

            if (this is MachineSlotController) {
                slotHeight += Screen.height/14;
                slotWidth  += Screen.height/14;
                
                if (gameObject.name == "InputSlot0") id = Int32.MaxValue;
                if (gameObject.name == "InputSlot1") id = Int32.MaxValue-1;
                if (gameObject.name == "FuelSlot") id = Int32.MaxValue-2;
                if (gameObject.name == "OutputSlot") id = Int32.MaxValue-3;
            }
                
            GameObject newGO = new GameObject("Icon" + id);
            newGO.transform.SetParent(gameObject.transform);
            newGO.AddComponent<InventorySlotDragHandler>();
            
            // Background (hitbox) image 
            Image image = newGO.AddComponent<Image>();
            (newGO.transform as RectTransform).sizeDelta = new Vector2(slotWidth, slotHeight);
            image.transform.localPosition = new Vector3(0, 0, 0);
            image.color = new Color(0, 0, 0, 0);
            
            // Sprite image
            GameObject imageGO = new GameObject("Image" + id);
            imageGO.transform.SetParent(newGO.transform);
            
            // Initialise image and text
            setupImage(imageGO);
            setupText(this.gameObject);

            // Initialise rollover object
            rolloverObject.GetComponentInChildren<Text>().font = assetManager.FontHelveticaNeueBold;
            rolloverObject.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
            rolloverObjectText = rolloverObject.GetComponentInChildren<Text>();
        }

        public void OnPointerEnter(PointerEventData pointerEventData) {
            setHighlightLocation(transform.position.x, transform.position.y);

            if (!mouseOver) {
                // Mouse entry
                mouseOver = true;
                mouseEntryTime = Time.realtimeSinceStartup;
            }
        }

        public void OnPointerExit(PointerEventData pointerEventData) {
            mouseOver = false;
            rolloverState = false;
            rolloverObject.SetActive(false);
            highlightObject.SetActive(false);
        }

        private void Update() {
            if (mouseOver) {
                if ((Time.realtimeSinceStartup - rolloverTime) > mouseEntryTime && storedItem.IsPresent()) {
                    if (!rolloverState) {
                        rolloverState = true;
                        rolloverObject.SetActive(true);
                        rolloverPosition = Input.mousePosition;
                        setRolloverLocation(Input.mousePosition.x, Input.mousePosition.y + slotHeight / 6,
                            storedItem.Get().GetName());
                    } else if (Input.mousePosition != rolloverPosition) {
                        rolloverObject.SetActive(false);
                        rolloverState = false;
                        mouseEntryTime = Time.realtimeSinceStartup;
                    }
                }
            }
        }

        private void setRolloverLocation(float x, float y, string inputText) {
            rolloverObject.transform.position = new Vector2(x, y);
            rolloverObjectText.text = inputText;

            // Set box to width of word
            RectTransform rect = rolloverObject.transform as RectTransform;
            rect.sizeDelta = new Vector2(rolloverObjectText.preferredWidth + slotWidth/8, slotHeight/5);
        }

        private void setHighlightLocation(float x, float y) {
            highlightObject.SetActive(true);
            highlightObject.transform.position = new Vector2(x, y);
            (highlightObject.transform as RectTransform).sizeDelta = (this.transform as RectTransform).sizeDelta;
        }

        public void setID(int id) {
            this.id = id;
        }

        public int getId() {
            return id;
        }

        public void SetStoredItem(Optional<InventoryItem> item) {
            this.storedItem = item;
            //TODO: GetChild(1) is a hack, fix it.
            Image image = gameObject.transform.GetChild(1).GetComponentsInChildren<Image>()[1];
            Text text = gameObject.GetComponentInChildren<Text>();

            // TODO: sub-optimal, fix it.
            if (gameObject.name == "FuelSlot" && storedItem.IsPresent()) {
                if (storedItem.Get().GetQuantity() == 0) storedItem = Optional<InventoryItem>.Empty();
            }

            if (!this.storedItem.IsPresent()) {
                image.enabled = false;
                text.enabled = false;
            } else {
                image.sprite = null;
                image.sprite = assetManager.GetItemSprite(item.Get().GetId());
                text.text = item.Get().GetQuantity().ToString();

                image.enabled = true;
                text.enabled = true;
            }
        }

        private Text setupText(GameObject obj) {
            GameObject textObj = new GameObject("Text" + id);
            textObj.transform.parent = this.transform;
            Text text = textObj.AddComponent<Text>();

            text.font = assetManager.FontHelveticaNeueBold;
            text.transform.localPosition = new Vector3(0, -originalSlotHeight/6, 0);
            text.color = assetManager.ColourOffWhite;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = "";
            text.raycastTarget = false;
            text.fontSize = assetManager.QuantityFieldFontSize;
            text.enabled = false;

            return text;
        }

        private Image setupImage(GameObject obj) {
            Image image = obj.AddComponent<Image>();
            image.transform.localPosition = new Vector3(0, slotHeight/8, 0);
            image.enabled = false;
            image.rectTransform.sizeDelta = new Vector2(slotWidth/3, slotHeight/3);
            image.raycastTarget = false;
            return image;
        }

        public void OnDrop(GameObject droppedObject) {
            RectTransform invPanel = transform as RectTransform;

            InventorySlotController source = droppedObject.GetComponent<InventorySlotController>();
            InventorySlotController destination = gameObject.GetComponent<InventorySlotController>();

            if (source == destination) {
                // Dragging to same slot
                return;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(invPanel, Input.mousePosition)) {
                if (destination.storedItem.IsPresent()) {
                    // Move to occupied slot
                    Optional<InventoryItem> temp = destination.storedItem;

                    destination.SetStoredItem(source.storedItem);
                    source.SetStoredItem(temp);
                } else {
                    // Move to empty slot
                    destination.SetStoredItem(source.storedItem);
                    source.SetStoredItem(Optional<InventoryItem>.Empty());
                }

                MachineController machineController = GameObject.Find("MachineCanvas").GetComponent<MachineController>();
                // If being added from a machine, decrement the machine's inputs
                // Or in the other cases, add to Inventory, remove from Machine slots
                if (source.name == "OutputSlot") {

                    Optional<InventoryItem> item = gameManager.machineStore.GetState().grid[machineController.machineLocation].output;
                    // If the target slot is non-empty and not of the same type
                    if (!source.storedItem.IsPresent() || source.storedItem.Get().GetId() == item.Get().GetId()) {
                        gameManager.machineStore.Dispatch(new ConsumeInputs(machineController.machineLocation));
                        gameManager.inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.Get().GetId(),
                            item.Get().GetQuantity(), item.Get().GetName(), destination.id));
                    } else {
                        Optional<InventoryItem> temp = destination.storedItem;
                        destination.SetStoredItem(source.storedItem);
                        source.SetStoredItem(temp);
                    }

                } else if (source.name == "InputSlot0") {
                    Optional<InventoryItem> item = gameManager.machineStore.GetState().grid[machineController.machineLocation].leftInput;

                    if (!source.storedItem.IsPresent()) {
                        gameManager.inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.Get().GetId(), item.Get().GetQuantity(), item.Get().GetName(), destination.id));
                        gameManager.machineStore.Dispatch(new ClearLeftInput(machineController.machineLocation));
                    } else {
                        if (source.storedItem.Get().GetId() == destination.storedItem.Get().GetId()) {
                            gameManager.inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.Get().GetId(), item.Get().GetQuantity(), item.Get().GetName(), destination.id));
                            gameManager.machineStore.Dispatch(new ClearLeftInput(machineController.machineLocation));
                        } else {
                            gameManager.machineStore.Dispatch(new SetLeftInput(machineController.machineLocation, source.storedItem.Get()));
                            gameManager.inventoryStore.Dispatch(new RemoveItemFromStackInventory(source.storedItem.Get().GetId(), 
                                source.storedItem.Get().GetQuantity(), destination.id));
                            gameManager.inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.Get().GetId(), item.Get().GetQuantity(), item.Get().GetName(), destination.id));
                        }
                    }

                } else if (source.name == "InputSlot1") {
                    Optional<InventoryItem> item = gameManager.machineStore.GetState().grid[machineController.machineLocation].rightInput;

                    if (!source.storedItem.IsPresent()) {
                        gameManager.inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.Get().GetId(), item.Get().GetQuantity(), item.Get().GetName(), destination.id));
                        gameManager.machineStore.Dispatch(new ClearRightInput(machineController.machineLocation));
                    } else {
                        if (source.storedItem.Get().GetId() == destination.storedItem.Get().GetId()) {
                            gameManager.inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.Get().GetId(), item.Get().GetQuantity(), item.Get().GetName(), destination.id));
                            gameManager.machineStore.Dispatch(new ClearRightInput(machineController.machineLocation));
                        } else {
                            gameManager.machineStore.Dispatch(new SetRightInput(machineController.machineLocation, source.storedItem.Get()));
                            gameManager.inventoryStore.Dispatch(new RemoveItemFromStackInventory(source.storedItem.Get().GetId(), 
                                source.storedItem.Get().GetQuantity(), destination.id));
                            gameManager.inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.Get().GetId(), item.Get().GetQuantity(), item.Get().GetName(), destination.id));
                        }
                    }

                } else if (source.name == "FuelSlot") {
                    GameObject.Find("FuelSlot").GetComponent<InventorySlotController>()
                        .SetStoredItem(Optional<InventoryItem>.Empty());

                    Optional<InventoryItem> item = gameManager.machineStore.GetState().grid[machineController.machineLocation].fuel;
                    gameManager.inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.Get().GetId(),
                        item.Get().GetQuantity(), item.Get().GetName(), destination.id));

                    gameManager.machineStore.Dispatch(new ClearFuel(machineController.machineLocation));

                } else {
                    this.gameManager.inventoryStore.Dispatch(new SwapItemLocations(source.id, destination.id,
                        destination.storedItem, source.storedItem));
                }
            }
        }
    }
}
