using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class create : MonoBehaviour
{
    //�ó��������ɵĹ���
    public GameObject targetEnemy;
    public CubeTest scriptPrefab;// ��Ҫ���صĽű���Ԥ��
    //���ɹ����������
    public int enemyTotalNum;
    //���ɹ����ʱ����
    public float intervalTime;
    //���ɹ���ļ�����
    private int enemyCounter;

    // Use this for initialization
    void Start()
    {
        //��ʼʱ���������Ϊ0��
        enemyCounter = 0;
        //�ظ����ɹ���
        InvokeRepeating("CreatEnemy", 0.5F, intervalTime);
    }

    // Update is called once per frame
    void Update()
    {

    }
    //���������ɹ���
    private void CreatEnemy()
    {
        //���������
        Vector3 suiji = this.transform.position;
        suiji.x = this.transform.position.x + Random.Range(0.0f, 5.0f);
        suiji.z = this.transform.position.y + Random.Range(0.0f, 5.0f);
        suiji.y = -0.35f;
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(180.0f, 270.0f), 0);
        GameObject newObject = Instantiate(targetEnemy, suiji, randomRotation);
        CubeTest scriptComponent = newObject.AddComponent<CubeTest>();
        enemyCounter++;
        Debug.Log("������������꣺" + suiji);
        //��������ﵽ���ֵ
        if (enemyCounter == enemyTotalNum)
        {
            //ֹͣˢ��
            CancelInvoke();
        }
    }
}

