Decisões de Arquitetura
-----------------

A. A Herança da Classe (class GameStoreInfraStack : Stack)
Linha: public class GameStoreInfraStack : Stack

O que faz: No AWS CDK, uma Stack representa uma unidade de implantação que é convertida em um CloudFormation template por baixo dos panos.

Organizar a infraestrutura em classes orientadas a objetos permite criar "construtores de infraestrutura" reutilizáveis. Se amanhã a Raid Hut precisar subir 5 lojas de jogos diferentes em regiões da AWS distintas, basta instanciar essa classe passando parâmetros diferentes, evitando duplicação de código (DRY - Don't Repeat Yourself).

-----------------

B. A Chave de Idempotência no DynamoDB (PartitionKey = new Attribute { Name = "IdempotencyKey", ... })
Linha: PartitionKey = new Attribute { Name = "IdempotencyKey", Type = AttributeType.STRING }

O que faz: Define a chave primária (Partition Key) da tabela NoSQL como IdempotencyKey.

Referência do Projeto 1: Como garantir que transações de loja não duplicarem itens? Aqui estamos cimentando isso na camada de dados. Ao definir o banco com essa chave, garantimos que buscas e gravações de verificação de idempotência ocorram em complexidade O(1) instantânea.

-----------------

C. O Modo de Cobrança On-Demand (BillingMode = BillingMode.PAY_PER_REQUEST)
Linha: BillingMode = BillingMode.PAY_PER_REQUEST

O que faz: Configura o DynamoDB para o modo sob demanda, em vez de provisionar capacidade fixa de leitura/escrita.

Se um grande evento é lançado em um jogo e milhares de jogadores entram na loja ao mesmo tempo, um banco com capacidade fixa engasgaria e
derrubaria o sistema (ou exigiria intervenção manual). O modo PAY_PER_REQUEST faz a AWS escalar a capacidade do banco instantaneamente de forma automática, aguentando picos de tráfego sem que você precise se preocupar em adivinhar o dimensionamento prévio.

-----------------

D. O Princípio do Menor Privilégio na Segurança (inventoryTable.GrantReadWriteData(storeLambda))
Linha: inventoryTable.GrantReadWriteData(storeLambda);

O que faz: Configura automaticamente as políticas de segurança IAM (Identity and Access Management) para permitir que apenas essa Lambda específica leia e escreva nessa tabela específica.

Escrever políticas IAM manualmente em JSON/YAML é propenso a erros graves de segurança (como dar permissão total * por preguiça). O CDK encapsula isso em métodos expressivos, aplicando estritamente o Princípio do Menor Privilégio (o microsserviço só tem acesso ao que estritamente precisa para rodar).

-----------------

E. A Visibilidade de Custo no PR (O pedido da vaga)
No mundo real do AWS CDK, usamos ferramentas como o Infracost ou o AWS CDK Cost Explorer Plugin dentro do pipeline do GitHub Actions.

Quando um desenvolvedor abre um Pull Request alterando a infraestrutura (por exemplo, adicionando um banco maior ou instâncias pesadas), o robô do CI/CD roda um relatório automático e comenta diretamente no PR: "Atenção: esta alteração aumentará o custo estimado da AWS em $45.00/mês". Isso impede surpresas no fim do mês na fatura da nuvem antes mesmo de o código ir para produção!loudFormation template