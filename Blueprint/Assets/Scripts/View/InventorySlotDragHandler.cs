﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Controller;
using Model;
using Model.Action;
using Model.State;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotDragHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private GameObject foregroundObject;
    private Image foregroundImage;
    private bool mouseOver = false;
    private bool dragging = false;
    private bool splitting = false;
    private GraphicRaycaster raycaster;
    private GraphicRaycaster secondaryCanvasRaycaster;
    private EventSystem eventSystem;
    private GameObject dragObject;
    private InventoryController inventoryController;
    private InventoryController machineInventoryController;
    private InventorySlotController inventorySlotController;
    private int newQuantity;

    private void Start() {
        raycaster = GetComponentInParent<GraphicRaycaster>();

        // Retrieve components
        eventSystem = GetComponent<EventSystem>();
        inventoryController = GameObject.Find("InventoryUICanvas").GetComponent<InventoryController>();
        machineInventoryController = GameObject.Find("MachineInventoryCanvas").GetComponent<InventoryController>();
        inventorySlotController = gameObject.transform.parent.GetComponent<InventorySlotController>();
        foregroundObject = GameObject.Find("Canvii/DragCanvas/Drag");
        foregroundImage = foregroundObject.GetComponent<Image>();

        // Reset DragDestination
        if (inventorySlotController.id == 0) inventoryController.DragDestination = -1;
    }

    private void Update() {   
        // Update secondaryCanvasRaycaster for Machine UI
        Transform parentCanvasObject = this.gameObject.transform.parent.parent; 
        if (parentCanvasObject.name == "MachineInventoryCanvas") {
            secondaryCanvasRaycaster = GameObject.Find("MachineCanvas").GetComponent<GraphicRaycaster>();
        } else if (parentCanvasObject.name == "MachineCanvas") {
            secondaryCanvasRaycaster = GameObject.Find("MachineInventoryCanvas").GetComponent<GraphicRaycaster>();
        }

        // Used by Goal UI
        if (GameManager.Instance().uiStore.GetState().Selected == UIState.OpenUI.Goal)
            secondaryCanvasRaycaster = GameObject.Find("GoalCanvas").GetComponent<GraphicRaycaster>();

        // DRAG
        // When left mouse button is down
        if (Input.GetMouseButtonDown(0)) {
            // End drag behaviour
            if (dragging) {
                // Raycast to determine new slot
                PointerEventData ped = new PointerEventData(eventSystem);
                ped.position = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(ped, results);
                
                // If no results, try secondary canvas
                if (results.Count == 0) {
                   secondaryCanvasRaycaster.Raycast(ped, results); 
                } 

                // Drop over slot?  
                InventorySlotController isc = null;
                foreach (RaycastResult result in results) {
                    if (result.gameObject.GetComponent<InventorySlotController>()) {
                        isc = result.gameObject.GetComponent<InventorySlotController>();
                    } 
                }

                if (isc != null) {
                    if (!splitting) {
                        // Dragging
                        if (isc is MachineSlotController) {
                            if (gameObject.transform.parent.name != "OutputSlot" && 
                                    (isc.name != "InputSlot0" || isc.name != "InputSlot1" || isc.name != "FuelSlot")) {
                                
                                isc.GetComponentInParent<MachineSlotController>().OnDrop(dragObject, false, newQuantity);
                            } else {
                                inventorySlotController.SetStoredItem(inventorySlotController.storedItem);
                            }
                        } else if (isc is GoalSlotController) {
                            isc.GetComponentInParent<GoalSlotController>().OnDrop(dragObject, false);
                        } else {
                            isc.OnDrop(dragObject);
                        }
                    } else {
                        // Splitting 
                        InventoryItem item = inventorySlotController.storedItem.Get();
                        string sourceSlot = gameObject.transform.parent.name;
                        
                        if (isc is MachineSlotController) {
                            // Into machine
                            isc.GetComponentInParent<MachineSlotController>().OnDrop(dragObject, true, newQuantity);
                        } else {
                            // Into inventory slot
                            if (isc.storedItem.IsPresent()) {
                                InventoryItem originalItem = inventorySlotController.storedItem.Get();
                                
                                // Into occupied slot...
                                if (isc.storedItem.Get().GetId() == inventorySlotController.storedItem.Get().GetId()) {
                                    // ... of the same type 
                                    GameManager.Instance().inventoryStore.Dispatch(new AddItemToInventoryAtHex(originalItem.GetId(), 
                                    newQuantity, originalItem.GetName(), isc.id));
                                } else {
                                    // ... of a different type
                                    if (sourceSlot != "InputSlot0" && sourceSlot != "InputSlot1" && sourceSlot != "FuelSlot") {
                                        GameManager.Instance().inventoryStore.Dispatch(new AddItemToInventoryAtHex(originalItem.GetId(), 
                                        newQuantity, originalItem.GetName(), inventorySlotController.id));
                                    } else {
                                        // Cancel split if from machine slot
                                        Vector2 machineLocation = (inventorySlotController as MachineSlotController).MachineController.machineLocation;
                                        InventoryItem unDropItem = new InventoryItem(originalItem.GetName(), originalItem.GetId(), originalItem.GetQuantity() + newQuantity);
                                        
                                        if (sourceSlot == "FuelSlot") GameManager.Instance().machineStore.Dispatch(new SetFuel(machineLocation, Optional<InventoryItem>.Of(unDropItem)));
                                        if (sourceSlot == "InputSlot0") GameManager.Instance().machineStore.Dispatch(new SetLeftInput(machineLocation, unDropItem));
                                        if (sourceSlot == "InputSlot1") GameManager.Instance().machineStore.Dispatch(new SetRightInput(machineLocation, unDropItem));
                                    }
                                }
                            } else {
                                // Into unoccupied slot
                                GameManager.Instance().inventoryStore.Dispatch(new AddItemToInventoryAtHex(item.GetId(), newQuantity, item.GetName(), isc.id));
                            }
                        }
                    }
                    
                    inventoryController.DragDestination = isc.id;
                } else {
                    string name = gameObject.transform.parent.name; 
                    
                    // Drop item outside the inventory while splitting
                    if (splitting) {
                        InventoryItem originalItem = inventorySlotController.storedItem.Get();
                        
                        if (name != "FuelSlot" && name != "InputSlot0" && name != "InputSlot1") {
                            GameManager.Instance().inventoryStore.Dispatch(new AddItemToInventoryAtHex(originalItem.GetId(), 
                                newQuantity, originalItem.GetName(), inventorySlotController.id));
                        } else {
                            Vector2 machineLocation = (inventorySlotController as MachineSlotController).MachineController.machineLocation;
                            InventoryItem unDropItem = new InventoryItem(originalItem.GetName(), originalItem.GetId(), originalItem.GetQuantity() + newQuantity);
                            
                            if (name == "FuelSlot") GameManager.Instance().machineStore.Dispatch(new SetFuel(machineLocation, Optional<InventoryItem>.Of(unDropItem)));
                            if (name == "InputSlot0") GameManager.Instance().machineStore.Dispatch(new SetLeftInput(machineLocation, unDropItem));
                            if (name == "InputSlot1") GameManager.Instance().machineStore.Dispatch(new SetRightInput(machineLocation, unDropItem));
                        }
                    } 
                    
                    // Populate output slot when item is dropped outside
                    if (name == "OutputSlot") inventorySlotController.SetStoredItem(inventorySlotController.storedItem);
                }

                if (!splitting) {
                    endDrag();
                } else {
                    endSplit();
                } 
            }
            
            // Begin drag behaviour
            if (mouseOver && !dragging && !inventoryController.DraggingInvItem &&
                inventorySlotController.storedItem.IsPresent() && (inventorySlotController.id != inventoryController.DragDestination)) {
                
                beginDrag();
            }
        }

        // SPLIT    
        // When right mouse button is down
        InventoryItem currentItem = inventorySlotController.storedItem.Get();
        if (Input.GetMouseButtonDown(1) && mouseOver && currentItem.GetQuantity() > 1 && !inventoryController.DraggingInvItem
            && gameObject.transform.parent.name != "OutputSlot") {
            
            newQuantity = (int) currentItem.GetQuantity() / 2;
            
            if (inventorySlotController is MachineSlotController) {
                Vector2 machineLocation = (inventorySlotController as MachineSlotController).MachineController.machineLocation;
                //currentItem.SetQuantity(currentItem.GetQuantity() - newQuantity);
                InventoryItem tempItem = new InventoryItem(currentItem.GetName(), currentItem.GetId(), currentItem.GetQuantity() - newQuantity);
                
                if (gameObject.transform.parent.name == "InputSlot0") {
                    GameManager.Instance().machineStore.Dispatch(new SetLeftInput(machineLocation, tempItem));
                    inventorySlotController.SetStoredItem(Optional<InventoryItem>.Of(tempItem)); 
                    
                } else if (gameObject.transform.parent.name == "InputSlot1") {
                    GameManager.Instance().machineStore.Dispatch(new SetRightInput(machineLocation, tempItem));
                    inventorySlotController.SetStoredItem(Optional<InventoryItem>.Of(tempItem)); 
                    
                } else if (gameObject.transform.parent.name == "FuelSlot") {
                    GameManager.Instance().machineStore.Dispatch(new SetFuel(machineLocation, Optional<InventoryItem>.Of(tempItem)));
                    inventorySlotController.SetStoredItem(Optional<InventoryItem>.Of(tempItem));
                }

            } else {
                // Remove drag quantity from source hex
                GameManager.Instance().inventoryStore.Dispatch(new RemoveItemFromStackInventory(currentItem.GetId(), 
                newQuantity, inventorySlotController.id));
            }

            beginSplit();
        }
        
        // Reset drag destination once object has been placed
        if (mouseOver && inventorySlotController.id == inventoryController.DragDestination) {
            inventoryController.DragDestination = -1;
        }

        // Icon follows mouse when left mouse button not down
        if (dragging) {
            foregroundObject.transform.position = Input.mousePosition;
        }
    }

    private void beginDrag() {
        dragging = true;
        inventoryController.DraggingInvItem = true;
                
        transform.parent.SetSiblingIndex(1);
        transform.position = Input.mousePosition;
        Sprite originalSprite = gameObject.transform.GetChild(0).GetComponent<Image>().sprite;
        Image originalImage = gameObject.transform.GetChild(0).GetComponent<Image>();
        gameObject.transform.parent.GetComponentInChildren<Text>().text = "";

        RectTransform rect = transform as RectTransform;
        foregroundImage.enabled = true;
        foregroundImage.rectTransform.sizeDelta = new Vector2((originalImage.transform as RectTransform).sizeDelta.x,
            (originalImage.transform as RectTransform).sizeDelta.y);
        originalImage.enabled = false;

        // NOTE: unloading & reloading sprite solves resizing issues
        foregroundImage.sprite = null;
        foregroundImage.sprite = originalSprite;
        
        dragObject = gameObject.transform.parent.gameObject;
    }

    private void endDrag() {
        dragging = false;
        inventoryController.DraggingInvItem = false;
        
        transform.localPosition = new Vector3(0, 0, 0);
        foregroundImage.enabled = false;
        
        inventoryController.RedrawInventory();
        machineInventoryController.RedrawInventory();

        string name = gameObject.transform.parent.name; 
        if (name == "FuelSlot" || name == "InputSlot0" || name == "InputSlot1") 
            inventorySlotController.SetStoredItem(inventorySlotController.storedItem);
    }

    private void beginSplit() {
        splitting = true;
        dragging = true;
        inventoryController.DraggingInvItem = true;
                
        transform.parent.SetSiblingIndex(1);
        Sprite originalSprite = gameObject.transform.GetChild(0).GetComponent<Image>().sprite;
        Image originalImage = gameObject.transform.GetChild(0).GetComponent<Image>();

        // Foreground object
        RectTransform rect = transform as RectTransform;
        foregroundImage.enabled = true;
        foregroundImage.rectTransform.sizeDelta = new Vector2((originalImage.transform as RectTransform).sizeDelta.x,
            (originalImage.transform as RectTransform).sizeDelta.y);
        
        // NOTE: unloading & reloading sprite solves resizing issues
        foregroundImage.sprite = null;
        foregroundImage.sprite = originalSprite;
        
        dragObject = gameObject.transform.parent.gameObject;
    }
    
    private void endSplit() {
        splitting = false;
        endDrag();
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
        if (!mouseOver) mouseOver = true;
    }
    
    public void OnPointerExit(PointerEventData pointerEventData) {
        if (mouseOver) mouseOver = false;
    }
}
