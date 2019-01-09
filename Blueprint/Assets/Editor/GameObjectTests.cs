﻿using System.IO;
using NUnit.Framework;
using UnityEngine;

public class GameObjectTests {
    private string testJsonsLocation = "Assets/Editor/TestJsons/";

    private string getFilepath(string filename) {
        return string.Concat(testJsonsLocation, filename);
    }

    // Serializes a single GameObjectEntry
    [Test]
    public void TestSerializeSingleItem() {
        using (StreamReader r = new StreamReader(getFilepath("single-item.json"))) {
            string json = r.ReadToEnd();

            GameObjectEntry entry = JsonUtility.FromJson<GameObjectEntry>(json);

            // Assert fields are correct
            Assert.That(entry.item_id, Is.EqualTo(1));
            Assert.That(entry.name, Is.EqualTo("wood"));
            Assert.That(entry.type, Is.EqualTo(1));
        }
    }
    
    // Serializes a single GameObjectEntry containing a recipe list
    [Test]
    public void TestSerializeSingleItemWithList() {
        using (StreamReader r = new StreamReader(getFilepath("single-item-with-list.json"))) {
            string json = r.ReadToEnd();

            GameObjectEntry entry = JsonUtility.FromJson<GameObjectEntry>(json);

            // Assert fields are correct
            Assert.That(entry.item_id, Is.EqualTo(8));
            Assert.That(entry.name, Is.EqualTo("steel"));
            Assert.That(entry.type, Is.EqualTo(4));
            
            // Asserts recipe entry is correct
            Assert.That(entry.recipe[0].item_id, Is.EqualTo(4));
            Assert.That(entry.recipe[0].quantity, Is.EqualTo(1));
        }
    }

    // Serializes the whole item schema using GameObjectsHandler
    [Test]
    public void TestInitialiseGameObjectsHandler() {
        GameObjectsHandler goh = new GameObjectsHandler(getFilepath("item-schema-v1.json"));
        
        //Assert fields are correct
        Assert.That(goh.GameObjs.items.Count, Is.EqualTo(16));
        
        Assert.That(goh.GameObjs.items[0].item_id, Is.EqualTo(1));
        Assert.That(goh.GameObjs.items[0].name, Is.EqualTo("wood"));
        Assert.That(goh.GameObjs.items[0].type, Is.EqualTo(1));
        
        Assert.That(goh.GameObjs.items[15].item_id, Is.EqualTo(16));
        Assert.That(goh.GameObjs.items[15].name, Is.EqualTo("dune buggy"));
        Assert.That(goh.GameObjs.items[15].type, Is.EqualTo(5));
        Assert.That(goh.GameObjs.items[15].blueprint[0].item_id, Is.EqualTo(9));
        Assert.That(goh.GameObjs.items[15].blueprint[0].quantity, Is.EqualTo(4));
        Assert.That(goh.GameObjs.items[15].blueprint[1].item_id, Is.EqualTo(10));
        Assert.That(goh.GameObjs.items[15].blueprint[1].quantity, Is.EqualTo(1));
    }
}