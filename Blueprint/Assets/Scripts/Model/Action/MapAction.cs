using System.Numerics;
using Vector2 = UnityEngine.Vector2;

namespace Model.Action {
    public interface MapVisitor {
        void visit(PlaceItem placeItem);
        void visit(CollectItem collectItem);
    }

    public abstract class MapAction : Action {
        public abstract void Accept(MapVisitor visitor);
    }
    
    /* Place an item at grid position */
    public class PlaceItem: MapAction {
        public readonly Vector2 position;
        public readonly int itemID;
        
        public PlaceItem(Vector2 position, int itemID) {
            this.position = position;
            this.itemID = itemID;
        }

        public override void Accept(MapVisitor visitor) {
            visitor.visit(this);
        }
    }
   
    /* Collect an item from a grid position */
    public class CollectItem: MapAction {
        public readonly Vector2 position;
        
        public CollectItem(Vector2 position) {
            this.position = position;
        }

        public override void Accept(MapVisitor visitor) {
            visitor.visit(this);
        }
    }

}