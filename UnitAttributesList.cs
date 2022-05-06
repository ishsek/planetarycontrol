using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttributesList
{
    
    Dictionary<string, float> attributes = new Dictionary<string, float>();
    
    public Dictionary<string,float> initialize(string type){
        switch(type){
            case "soldier":
                attributes.Add("attackPower" , 5f);
                attributes.Add("armor" , 3.5f);
                attributes.Add("speed" , 5f);
                attributes.Add("jumpHeight" , 2f);
                attributes.Add("fullHealth" , 100f);
                attributes.Add("vision" , 5f);
                return attributes;
                break;
            case "doctor":
                attributes.Add("attackPower" , 3.5f);
                attributes.Add("armor" , 4f);
                attributes.Add("speed" , 5f);
                attributes.Add("jumpHeight" , 2f);
                attributes.Add("fullHealth" , 80f);
                attributes.Add("vision" , 5f);
                return attributes;
                break;
            case "tank":
                attributes.Add("attackPower" , 2.5f);
                attributes.Add("armor" , 6f);
                attributes.Add("speed" , 2f);
                attributes.Add("jumpHeight" , 1f);
                attributes.Add("fullHealth" , 200f);
                attributes.Add("vision" , 3f);
                return attributes;
                break;
            case "sniper":
                attributes.Add("attackPower" , 15f);
                attributes.Add("armor" , 1.5f);
                attributes.Add("speed" , 3f);
                attributes.Add("jumpHeight" , 3f);
                attributes.Add("fullHealth" , 50f);
                attributes.Add("vision" , 15f);
                return attributes;
                break;
            default:
                return attributes;
                break;
        }
    }
    
}
