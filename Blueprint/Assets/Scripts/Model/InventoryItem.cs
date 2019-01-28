using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public class InventoryItem : ScriptableObject {

        private int id;
        private int quantity;
        private string type;
    
        public InventoryItem(int id, string type, int quantity) {
            this.id = id;
            this.type = type;
            this.quantity = quantity;
        }
    
        public int GetId() {
            return this.id;
        }
    
        public string GetItemType() {
            return this.type;
        }
    
        public void SetItemType(string type) {
            this.type = type;
        }
    
        public int GetQuantity() {
            return quantity;
        }
        
        public void SetQuantity(int quantity) {
            this.quantity = quantity;
        }
	}
}