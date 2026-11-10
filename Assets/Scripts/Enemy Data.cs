using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Project/Data/Enemy Data")]

public class EnemyData : ScriptableObject
{
    [field: Header("Data")]
    [field: SerializeField] public string EnemyName { get; private set; } = "Skeleton";
    [field: SerializeField] public int MaxHealth{ get; private set; } = 100;
    [field: SerializeField] public float MoveSpeed{ get; private set; } = 3.5f;
    
    [field: Header("Combat")]
    [field: SerializeField] public int AttackDamage{ get; private set; } = 15;
    



}