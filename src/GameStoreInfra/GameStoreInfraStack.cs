using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using System.Collections.Generic;

namespace GameStoreInfra
{
public class GameStoreInfraStack : Stack
{
    public GameStoreInfraStack(Scope scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // 1. Criação do Banco NoSQL (DynamoDB) para o Inventário / Transações da Loja
        var inventoryTable = new Table(this, "GameInventoryTable", new TableProps
        {
            PartitionKey = new Attribute { Name = "IdempotencyKey", Type = AttributeType.STRING },
            BillingMode = BillingMode.PAY_PER_REQUEST, // Escalabilidade automática sob pico de carga
            RemovalPolicy = RemovalPolicy.DESTROY // Remove a tabela se o stack for destruído (ideal para ambientes de teste/dev)
        });

        // 2. Criação da Função AWS Lambda em .NET para processar as compras da loja
        var storeLambda = new Function(this, "StoreCheckoutFunction", new FunctionProps
        {
            Runtime = Runtime.NET_8,
            Code = Code.FromAsset("../src/StoreLambdaApp"), // Aponta para o diretório binário da Lambda .NET
            Handler = "StoreLambdaApp::StoreLambdaApp.Function::HandleAsync", // Método de entrada (EntryPoint) da Lambda
            Environment = new Dictionary<string, string>
            {
                { "INVENTORY_TABLE_NAME", inventoryTable.TableName } // Injeta o nome da tabela via variável de ambiente
            }
        });

        // 3. Concessão Automática de Permissões (IAM Security)
        inventoryTable.GrantReadWriteData(storeLambda);

        // 4. Output (Saída) visível no console após o deploy para referência
        new CfnOutput(this, "InventoryTableNameOutput", new CfnOutputProps
        {
            Value = inventoryTable.TableName,
            Description = "Nome da tabela de inventário da loja no DynamoDB"
        });
    }
}