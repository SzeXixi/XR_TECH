using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class create : MonoBehaviour
{
    //该出生点生成的怪物
    public GameObject targetEnemy;
    public CubeTest scriptPrefab;// 需要挂载的脚本的预设
    //生成怪物的总数量
    public int enemyTotalNum;
    //生成怪物的时间间隔
    public float intervalTime;
    //生成怪物的计数器
    private int enemyCounter;

    // Use this for initialization
    void Start()
    {
        //初始时，怪物计数为0；
        enemyCounter = 0;
        //重复生成怪物
        InvokeRepeating("CreatEnemy", 0.5F, intervalTime);
    }

    // Update is called once per frame
    void Update()
    {

    }
    //方法，生成怪物
    private void CreatEnemy()
    {
        //创建随机点
        Vector3 suiji = this.transform.position;
        suiji.x = this.transform.position.x + Random.Range(0.0f, 5.0f);
        suiji.z = this.transform.position.y + Random.Range(0.0f, 5.0f);
        suiji.y = -0.35f;
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(180.0f, 270.0f), 0);
        GameObject newObject = Instantiate(targetEnemy, suiji, randomRotation);
        CubeTest scriptComponent = newObject.AddComponent<CubeTest>();
        enemyCounter++;
        Debug.Log("生成物体的坐标：" + suiji);
        //如果计数达到最大值
        if (enemyCounter == enemyTotalNum)
        {
            //停止刷新
            CancelInvoke();
        }
    }
}

