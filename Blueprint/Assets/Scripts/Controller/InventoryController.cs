using System.Collections.Generic;
using System.IO;
using System.Net;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Model;
using Model.Action;
using Model.Redux;
using Model.State;
using Utils;
using View;
using Service;
using Service.Response;
using System.Threading.Tasks;

/* Attached to the player and controls inventory collection */
namespace Controller {
    public class InventoryController : MonoBehaviour, Subscriber<GameState> {
        [SerializeField] private GameObject itemLabel;
        private InventoryItem[] inventoryContents;
        private List<InventorySlotController> itemSlots;
        private const int size = 16;
        private ResponseGetInventory remoteInv;

        public void Start() {
            inventoryContents = new InventoryItem[size];
            itemSlots = GameObject.Find("GridPanel").GetComponentsInChildren<InventorySlotController>().ToList();
            UserCredentials user = GameManager.Instance().GetUserCredentials();
            GameManager.Instance().store.Subscribe(this);
            var blueprintApi = BlueprintAPI.DefaultCredentials();

            Task.Run(async () => {
                APIResult<ResponseGetInventory, JsonError> finalInventoryResponse = await blueprintApi.AsyncGetInventory(user);
                if (finalInventoryResponse.isSuccess()) {
                    remoteInv = finalInventoryResponse.GetSuccess();
                } else {
                    JsonError error = finalInventoryResponse.GetError();
                }
            }).GetAwaiter().GetResult();
            
            foreach (InventoryEntry entry in remoteInv.items) {
                GameManager.Instance().store.Dispatch(
                    new AddItemToInventory(entry.item_id, entry.quantity, GetItemName(entry.item_id)));
            }
            /*            
                        Task.Run(async () => {
                            try {
                                APIResult<Boolean, JsonError> response = await blueprintApi.AsyncDeleteInventory(user);

                                // Success case
                            } catch (WebException e) {
                                // Failure case
                                throw new System.Exception("Did not delete inventory.");
                            }    
                        }).GetAwaiter().GetResult();*/
        }

        public InventoryItem[] GetItems() {
            return inventoryContents;
        }

<<<<<<< HEAD
        public void StateDidUpdate(GameState state) {
            inventoryContents = state.inventoryState.inventoryContents;
            
            // Update UI based on new state
            inventoryContents.Where(x => x != null).Each((element, i) => {
                if (itemSlots[i].transform.childCount < 2) {
                    GameObject label = Instantiate(itemLabel, itemSlots[i].transform, false);
                    label.name = getSlotName(i);
                    label.GetComponent<Text>().text = $"{element.GetName()} ({element.GetQuantity()})";
                } else if (element.GetQuantity() > 0){
                    GameObject.Find(getSlotName(i)).GetComponentInChildren<Text>().text =
                        $"{element.GetName()} ({element.GetQuantity()})";
=======
        public int GetInventorySize() {
            return size;
        }

        public void AddItem(InventoryItem item) {
            if (IsSpace()) {
                InventoryItem slotItem = items[GetSlot(item)];
                if (slotItem != null) {
                    items[GetSlot(item)].SetQuantity(slotItem.GetQuantity() + 1);
>>>>>>> Add initial Machine Interaction classes
                } else {
                    GameObject.Find(getSlotName(i)).GetComponentInChildren<Text>().text = "";
                }
                
                // Change load order or UI elements for accessible hit-box
                GameObject dropButton = GameObject.Find(getButtonName(i));
                itemLabel.transform.SetSiblingIndex(0);
                dropButton.transform.SetSiblingIndex(1);
            });
        }
        
        public string GetItemName(int id) {
            GameObjectsHandler goh = GameObjectsHandler.WithRemoteSchema();
            return goh.GameObjs.items[id - 1].name;
        }

        public void CollectItem(Interactable focus, GameObject pickup) {
            // TODO: This destroy should be the role of the map state
            Destroy(pickup);
            GameManager.Instance().store.Dispatch(
                new AddItemToInventory(focus.GetId(), 1, focus.GetItemType()));
        }
        
        private string getSlotName(int id) {
            return "InventoryItemSlot " + id;
        }

        private string getButtonName(int id) {
            return "Button" + (id + 1);
        }
    }
}
