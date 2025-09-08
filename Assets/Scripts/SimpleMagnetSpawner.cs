using UnityEngine;

public class SimpleMagnetSpawner : MonoBehaviour
{
    public Magnet magnetPrefab;
    public KeyCode attractKey = KeyCode.A;
    public KeyCode repelKey = KeyCode.D;
    public GameManager gameManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Spawn(true); // left click attract
        }
        if (Input.GetMouseButtonDown(1))
        {
            Spawn(false); // right click repel
        }
        if (Input.GetKeyDown(attractKey))
        {
            ToggleClosest(true);
        }
        if (Input.GetKeyDown(repelKey))
        {
            ToggleClosest(false);
        }
    }

    void Spawn(bool attract)
    {
        if (!gameManager.CanSpawnMagnet())
        {
            Debug.Log("Maximum magnets reached!");
            return;
        }
        Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var m = Instantiate(magnetPrefab, world, Quaternion.identity);
        m.isAttract = attract;
        m.GetComponent<SpriteRenderer>().color = attract ? Color.blue : Color.red;
        m.name = attract ? "Magnet_Attract" : "Magnet_Repel";
    }

    void ToggleClosest(bool attract)
    {
        Magnet[] magnets = FindObjectsOfType<Magnet>();
        if (magnets.Length == 0) return;
        Magnet closest = magnets[0];
        float best = Vector2.Distance(closest.transform.position, Vector2.zero);
        foreach (var m in magnets)
        {
            float d = Vector2.Distance(m.transform.position, Vector2.zero);
            if (d < best) { best = d; closest = m; }
        }
        closest.isAttract = attract;
        closest.GetComponent<SpriteRenderer>().color = attract ? Color.blue : Color.red;
    }
}