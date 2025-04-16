using UnityEngine;

public static class Structures {
  public enum EqualityE{
    Equal = 0,    
    NotEqual = 1,    
    Greater = 2,    
    GreaterOrEqual = 3,    
    Less = 4,    
    LessOrEqual = 5,    
  }
    
  public static bool Compare(float a, float b, EqualityE equalityRule, float tolerance = 0.1f){
    return equalityRule switch{
      EqualityE.Equal => Mathf.Abs(a - b) <= tolerance,
      EqualityE.NotEqual => Mathf.Abs(a - b) > tolerance,
      EqualityE.Greater => a > b,
      EqualityE.GreaterOrEqual => a >= b,
      EqualityE.Less => a < b,
      EqualityE.LessOrEqual => a <= b,
      _ => false
    };
  }
}