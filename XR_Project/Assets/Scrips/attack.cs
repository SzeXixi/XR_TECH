using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.UI;

public class attack : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public Slider healthSlider;
    private InteractableUnityEventWrapper unityEvent;
    private MeshRenderer meshRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        unityEvent = GetComponent<InteractableUnityEventWrapper>();
        meshRenderer = GetComponent<MeshRenderer>();

        unityEvent.WhenSelect.AddListener(() =>
        {
            // �����ʱ�����ܵ��˺�
            TakeDamage(10);
        });
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            Destroy(gameObject); // ��������ֵС�ڵ���0ʱ���ٹ������
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage; // �����Ѫ
        healthSlider.value = currentHealth; // ����Ѫ����ʾ

        if (currentHealth <= 0)
        {
            unityEvent.enabled = false; // ����������ʱ���ý����¼�
        }
    }
}
