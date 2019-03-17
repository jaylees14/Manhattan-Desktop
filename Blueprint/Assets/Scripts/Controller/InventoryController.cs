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
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.Rendering;

/* Attached to the player and controls inventory collection */
namespace Controller {
    public class InventoryController : MonoBehaviour, Subscriber<InventoryState> {
        public Dictionary<int, List<HexLocation>> inventoryContents;
        private Dictionary<int, InventorySlotController> itemSlots;
        private GameManager gameManager;

        public void Start() {
            
            // Generate dictionary of item slots for fast lookup
            itemSlots = new Dictionary<int, InventorySlotController>();
            List<InventorySlotController> allSlots = GameObject.Find("InventoryUICanvas").GetComponentsInChildren<InventorySlotController>().ToList();
            foreach (InventorySlotController controller in allSlots) {
              itemSlots.Add(controller.getId(), controller);
            }
            
            UserCredentials user = GameManager.Instance().GetUserCredentials();
            BlueprintAPI blueprintApi = BlueprintAPI.DefaultCredentials();
            
            Task.Run(async () => {
                APIResult<ResponseGetInventory, JsonError> finalInventoryResponse = await blueprintApi.AsyncGetInventory(user);
                if (finalInventoryResponse.isSuccess()) {
                    ResponseGetInventory remoteInv = finalInventoryResponse.GetSuccess();
                    foreach (InventoryEntry entry in remoteInv.items) {
                        GameManager.Instance().inventoryStore.Dispatch(
                            new AddItemToInventory(entry.item_id, entry.quantity, GetItemName(entry.item_id)));
                    }
                } else {
                    JsonError error = finalInventoryResponse.GetError();
                }
            }).GetAwaiter().GetResult();
            
            GameManager.Instance().inventoryStore.Subscribe(this);
            
        }
        
        public void StateDidUpdate(InventoryState state) {
            inventoryContents = state.inventoryContents;
            redrawInventory();
        }

        public string GetItemName(int id) {
            GameObjectsHandler goh = GameObjectsHandler.WithRemoteSchema();
            return goh.GameObjs.items[id - 1].name;
        }
        
        public int GetItemType(int id) {
            GameObjectsHandler goh = GameObjectsHandler.WithRemoteSchema();
            return goh.GameObjs.items[id - 1].type;
        }

        public void redrawInventory() {
            // Clear slots
            foreach (KeyValuePair<int, InventorySlotController> slot in itemSlots) {
                slot.Value.SetStoredItem(Optional<InventoryItem>.Empty());
            }
            
            // Re-populate slots
            foreach (KeyValuePair<int, List<HexLocation>> element in inventoryContents) {
                foreach(HexLocation loc in element.Value) {
                    InventoryItem item = new InventoryItem(GetItemName(element.Key), element.Key, loc.quantity);
                    itemSlots[loc.hexID].SetStoredItem(Optional<InventoryItem>.Of(item));
                } 
            }
        }
    }
}
