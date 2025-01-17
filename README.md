﻿# Overview

 - This is a too simple `.NET` sample for verifing `Open AI Embeddings` by using `pgVector`
 - Screenshot
  ![screenshot](ConsoleApp1/assets/screenshot.png)

 # References and Thanks for `Open Source` and `Internet`
 - Why `OpenAI Embedding` return different vectors for the same text input?

    - https://community.openai.com/t/why-openai-embedding-return-different-vectors-for-the-same-text-input/144495
    - I am not sure if the above answer is accurate. 
    - I am not sure if the differences in vectors generated for the same content can specifically affect the distance or sorting results between vectors?

 - Retrieve the similarity scores of matching sorted `Ad-hoc` `vectors` with the preserved `embedding vectors` of contents.

    - https://github.com/gannonh/gpt3.5-turbo-pgvector

    - https://weibo.com/1727858283/MzNJjh3yW
```
微博正文

问：宝玉老师，我是智能制造领域的开发者，我想把 AIGC 应用到生产故障定位的场景中，比如：当前手机主板生产检测中发现故障，还需要人工根据pcb、layout、个人经验等信息给出判断，能否先把 pcb、layout 喂给 AIGC，然后当故障出现时人工再填写一些故障现象（作为prompt），得到一些维修建议。

答：
我觉得这应该是一个偏文档检索的AI需求，也是蛮好的一个应用AI的场景。

首先对于ChatGPT这类AI来说，它们只有很少甚至没有你所在领域的经验，这部分数据需要你提供给AI，才能让它帮到你。

所以要做这件事，你得先保证自己有一定量的文档库或者知识库，最好是结构化的文本数据。文档数据越全这件事越容易做。

然后要基于AI训练自己的数据通常有两种方式：fine tunes 和 embedding

这两个都可以达到目的，但有一些差别：

fine tunes通常是指的微调，也就是基于大模型之上加上自己的数据，但你无法改变大模型本身，只能是提供一些特定领域的数据参考，或者让它用特定的风格、格式回复问题。

如果你要用fine tunes训练自己的数据，你需要将自己的数据整理成格式化的prompt（提示） 和completion（完成）

例如：
prompt：如果手机黑屏怎么处理
completion：先检查电源，然后看显示屏，再……

等你的数据训练好了，如果用户在使用你的模型数据时，正好问到了你训练过的相似的prompt，那么可以直接得到对应的completion，如果没有匹配到，那还是没法有结果。

Embedding的话，你只是将自己的数据向量化，把文档的文本分块，每一块转换成1536维向量数字，数据还是存在你自己那里。当你需要检索或者提问时，先通过向量检索找到对应的文档，然后把找到的所有文档内容，连同你的问题一起交给AI，然后AI根据找到的资料，整理后返还给你。

举例来说，用户问：“手机黑屏怎么处理”
，然后你的系统将这段话转换成向量数组，系统去你的文档库里面找，结果找到匹配的文档1,2,3。系统将文档和用户的问题一起交给AI，跟AI说：有用户问了“手机黑屏怎么处理”这个问题，我给你找到了这几个技术文档，你帮我整理总结一下发给ta。

最后AI把找到的文档整理后发给用户。

再回到之前你问的问题，就你这种需求，我个人建议采用Embedding会比较好，几个原因：

1. Embedding能满足你的需求，可以根据问题找到想要的答案，并且是自然语言的交互，甚至不要求文档的语言，任何语言的文档都可以支持
2. Embedding对数据源的格式要求要低一些，一般的知识库文档网页就够了
3. Embedding很便宜，整个文档库都做一次向量化花不了多少钱
4. 很多现成的开源系统可以帮助你做这个事。

也不是说不可以用fine tunes，比如：
1. 你不差钱
2. 你不想麻烦去做Embedding、保存检索向量数据、搭建系统，你只想有个模型直接用

关于Embedding，给你推荐几篇我写过的微博：

网页链接

网页链接

网页链接

```
    
   - https://weibo.com/1727858283/MxxgolOyb

```
微博正文

@宝玉xp (<= 感谢)
3-16 14:38 来自 微博网页版 已编辑
上一篇 网页链接 介绍的Supabase技术文档接入AI搜索后，能支持自然语言，能写代码，甚至能debug，估计很多人好奇它是怎么实现的。

以前写过一篇介绍原理 网页链接 ，这里再针对Supabase的集成了ChatGPT的AI文档检索写一篇。

集成ChatGPT的AI文档检索要解决两个问题：1. 检索；2. 对话

比如说如图一所示，我问了一个问题：“如何使用Nextjs连接supalbase， 请用中文回复”

第一步要做的就是把所有满足这个输入条件的文章都找出来。但按照以前全文检索的思路，分词、再查找，几乎不可能，因为原始文档都是英文的，而我输入的是中文，根本无法按照关键字去检索。

这里用的是一种Embeddings的技术，OpenAI针对Embeddings提供了API，输入文本后，可以得到一串数字。

那么什么是Embeddings呢？

Embeddings是用来测量“关联性”的，可以用来：
- 搜索：看你搜索的问题和一组文本的相似度如何
- 建议：两种产品的相似度如何
- 分类：如何对文本进行分类

比如说下面有三个短语：
1. "狗咬耗子"
2. "犬类捕食啮齿动物"
3. "我家养了只狗"

那很明显1和2是类似的，即使它们之间没有任何相同的文字。那么怎么让计算机知道1和2很相似呢？

OpenAI 的 Embeddings 是如何工作的？

当然我们不需要关心背后的神经网络模型，Embeddings将文本压缩成分布式的连续值数据（vectors向量）。

如果我们把之前的短语对应的向量能画在坐标轴上，它看起来像图2那样。短句1和短句2会离得很近，而短句3和他们离得比较远。

OpenAI提供了Embeddings的API，可以事先将所有的文档转成文本向量数据，然后将结果存储到支持向量的数据库。如果你数据不大，存成csv文件，然后加载到内存，借助内存搜索也是一样的。具体可以参考Kindle GPT的实现：网页链接

当用户提问的时候，把用户的问题也借助Embeddings API也变成文本向量，然后使用向量搜索，就能找出来哪些结果是接近的。比如我提的问题，文档中的“Use Supabase with NextJS
”就很接近。

借助Embeddings，就能帮助用户检索到想要的结果了。（参考图3的流程图）

但这还不够，因为光检索到结果，只能给用户返回文档，而不能按照用户的要求返回中文，甚至生成代码。

这时候就要借助ChatGPT的和prompt了。

ChatGPT是一个AI聊天机器人，它有一个庞大的知识库，它能理解用户的指令，能写代码，但是它对你的文档却一无所知，所以用户在提问时，你需要把匹配到的文档，生成prompt，喂给ChatGPT，让ChatGPT将“用户的问题”、“搜索到的文档”结合自己的知识库，返回给用户最终的结果。

继续以图一中我的问题为例，给ChatGPT的prompt大概长这样：

----------------------
你是一个非常热情的 Supabase 代言人，热爱帮助他人！请根据以下来自 Supabase 文档的部分内容，仅使用这些信息以 Markdown 格式回答问题。如果你不确定且答案未在文档中明确给出，请说“抱歉，我不知道如何帮助您。”

上下文部分：
Create a NextJS app
Create a Next.js app using the create-next-app command.
Install the Supabase client library
cd my-app && npm install @supabase/supabase-js
// 此处略去若干字..............

问题："如何使用Nextjs连接supalbase， 请用中文回复"

以 Markdown 格式回答（如果有的话，包括相关代码片段）：
----------------------

有了这些信息，就足够ChatGPT帮助你按照Supabase上匹配的文档，给你回复甚至生成代码了。

参考文档：网页链接

如果你需要开源的ChatGPT文档检索回复的代码实现，可以参考 gpt3.5-turbo-pgvector 这个项目：
🔗 github.com/gannonh/gpt3.5-turbo-pgvector
长图

```

- `pgVector`
    
    https://github.com/pgvector/pgvector-dotnet

# Docker Run `PostgreSQL` Database Server
```
docker pull ankane/pgvector

docker run --name test-pgvector -e POSTGRES_PASSWORD=password01! -d -p 5432:5432 ankane/pgvector

```

# `PostgreSQL` Database preparation
- Use `pgAdmin` connect to server
    https://www.pgadmin.org/download/
```
#Address:
localhost
127.0.0.1

#Port:
5432
```

- create the database `pgVectors`:
```sql
create database pgVectors
```

- Create user/login for database `pgVectors`
```sql
-- Role: sa
-- DROP ROLE IF EXISTS sa;

CREATE ROLE sa WITH
  LOGIN
  SUPERUSER
  INHERIT
  CREATEDB
  CREATEROLE
  REPLICATION
  PASSWORD 'password01!';
```

- Switch to database `pgVectors` on `pgAmin` and then:
```sql
CREATE EXTENSION vector;
```

- Create the table named `ContentsEmbeddings`:
```sql
-- Why OpenAI Embedding return different vectors for the same text input?
-- https://community.openai.com/t/why-openai-embedding-return-different-vectors-for-the-same-text-input/144495
-- The table is designed for store all history distinct vectors/embeddings of every unique input content

-- Table: public.ContentsEmbeddings
-- DROP TABLE IF EXISTS public."ContentsEmbeddings";

CREATE TABLE IF NOT EXISTS public."ContentsEmbeddings"
(
    "ID" bigserial,
    "Content" character varying(65536) COLLATE pg_catalog."default",
    "ContentHash" character varying(64) COLLATE pg_catalog."default",
    "EarliestEmbeddingHash" character varying(64) COLLATE pg_catalog."default",
    "EarliestEmbedding" vector(1536),
    "EarliestEmbeddingCreateTime" timestamp without time zone,
    "EmbeddingHash" character varying(64) COLLATE pg_catalog."default",
    "Embedding" vector(1536),
    "EmbeddingCount" integer DEFAULT 1,
    "CreateTime" timestamp without time zone DEFAULT now(),
    "UpdateTime" timestamp without time zone DEFAULT now()
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."ContentsEmbeddings"
    OWNER to sa;
-- Index: Unique_btree_ContentsEmbeddingsHash

-- DROP INDEX IF EXISTS public."Unique_btree_ContentsEmbeddingsHash";

CREATE UNIQUE INDEX IF NOT EXISTS "Unique_btree_ContentsEmbeddingsHash"
    ON public."ContentsEmbeddings" USING btree
    ("ContentHash" COLLATE pg_catalog."default" ASC NULLS LAST, "EmbeddingHash" COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: ivfflat_embedding

-- DROP INDEX IF EXISTS public.ivfflat_embedding;

CREATE INDEX IF NOT EXISTS ivfflat_embedding
    ON public."ContentsEmbeddings" USING ivfflat
    ("Embedding")
    TABLESPACE pg_default;
```

- How to store all history of every unique input content?
    - Reference:    
        https://github.com/AwesomeYuer/OpenAI.Embeddings.pgvector.NET/blob/a722eedda15e88f142f09e113b8f49f75879be23/ConsoleApp1/Program.cs#L59


- Sample query `Preserved content's Vectors/Embeddings` matched and sorted by similarity with `(table v)`, that's the distance between `(table v)` and `Preserved content's Vectors/Embeddings`
```sql

-- Query order by ascending the distance between the vector of ad-hoc query key words's embedding and the vectors of preserved contents of embeddings in database
-- The `distance [0, 2]` means `1 - similarity [-1, 1]`
WITH
v
AS
(
    VALUES
    (
         -- 1536
         '[1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,11,11,1,1,1,11,1,1,1,1,1,1,1,1,1,11,1,1,1,1,1]'::vector
    )
)
,
T1
as
(
    SELECT
          "Content"
        , "EarliestEmbedding"   <-> (table v)       	as "EarliestL2Distance"
		, "Embedding"   <-> (table v)       			as "L2Distance"
        , "EarliestEmbedding"   <=> (table v)       	as "EarliestCosineDistance"
	    , "Embedding"   <=> (table v)       			as "CosineDistance"
	    , 1 - ("EarliestEmbedding"   <=> (table v)) 	as "EarliestCosineSimilarity"
	    , 1 - ("Embedding"   <=> (table v))       		as "CosineSimilarity"
        , "EarliestEmbeddingHash"	
        , (table v)                                 	as "AdHocQueryInputEmbedding"
        , "EmbeddingHash"
        , "UpdateTime"
        , "CreateTime"
    FROM
        "ContentsEmbeddings"
)
SELECT
      a.*
    , (a."L2Distance" - a."EarliestL2Distance")         as "DiffDistance"
FROM
    T1 a
ORDER BY
    --a."L2Distance"
	"CosineDistance"
	;

```

# Run/Debug program/project entry:
```

# Open Visual Studio solution at first, and then run or debug:
OpenAI.Embeddings.pgvector.NET\ConsoleApp1\ConsoleApp1.csproj

```

# Config your `Open AI` key in :
- Config file:
    `OpenAI.Embeddings.pgvector.NET\ConsoleApp1\.openai`
- Content: 
    - The all `sk-XXXXXXXX` are same as your `Open AI Key` on https://platform.openai.com/account/api-keys
    - The all `org-YYYYYYYY` are same as your `Organization ID` in the `Settings` on https://platform.openai.com/account/org-settings
    ```
    OPENAI_KEY=sk-XXXXXXXX
    OPENAI_API_KEY=sk-XXXXXXXX
    OPENAI_SECRET_KEY=sk-XXXXXXXX
    TEST_OPENAI_SECRET_KEY=sk-XXXXXXXX
    OPENAI_ORGANIZATION_ID=org-YYYYYYYY
    OPEN_AI_ORGANIZATION_ID=org-YYYYYYYY
    ORGANIZATION=org-YYYYYYYY
    ```

# OpenAI-DotNet

[![Discord](https://img.shields.io/discord/855294214065487932.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/xQgMW9ufN4)
[![NuGet version (OpenAI-DotNet)](https://img.shields.io/nuget/v/OpenAI-DotNet.svg?label=OpenAI-DotNet&logo=nuget)](https://www.nuget.org/packages/OpenAI-DotNet/)
[![NuGet version (OpenAI-DotNet-Proxy)](https://img.shields.io/nuget/v/OpenAI-DotNet-Proxy.svg?label=OpenAI-DotNet-Proxy&logo=nuget)](https://www.nuget.org/packages/OpenAI-DotNet-Proxy/)
[![Nuget Publish](https://github.com/RageAgainstThePixel/OpenAI-DotNet/actions/workflows/Publish-Nuget.yml/badge.svg)](https://github.com/RageAgainstThePixel/OpenAI-DotNet/actions/workflows/Publish-Nuget.yml)

A simple C# .NET client library for [OpenAI](https://openai.com/) to use use chat-gpt, GPT-4, GPT-3.5-Turbo and Dall-E though their RESTful API (currently in beta). Independently developed, this is not an official library and I am not affiliated with OpenAI. An OpenAI API account is required.

Forked from [OpenAI-API-dotnet](https://github.com/OkGoDoIt/OpenAI-API-dotnet).

More context [on Roger Pincombe's blog](https://rogerpincombe.com/openai-dotnet-api).

> This repository is available to transfer to the OpenAI organization if they so choose to accept it.

## Requirements

- This library targets .NET 6.0 and above.
- It should work across console apps, winforms, wpf, asp.net, etc.
- It should also work across Windows, Linux, and Mac.

## Getting started

### Install from NuGet

Install package [`OpenAI-DotNet` from Nuget](https://www.nuget.org/packages/OpenAI-DotNet/).  Here's how via command line:

```powershell
Install-Package OpenAI-DotNet
```

> Looking to [use OpenAI-DotNet in the Unity Game Engine](https://github.com/RageAgainstThePixel/com.openai.unity)? Check out our unity package on OpenUPM:
>
>[![openupm](https://img.shields.io/npm/v/com.openai.unity?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.openai.unity/)

## Documentation

### Table of Contents

- [Authentication](#authentication)
- [Azure OpenAI](#azure-openai)
- [OpenAI API Proxy](#openai-api-proxy) :new:
- [Models](#models)
  - [List Models](#list-models)
  - [Retrieve Models](#retrieve-model)
  - [Delete Fine Tuned Model](#delete-fine-tuned-model)
- [Completions](#completions)
  - [Streaming](#completion-streaming)
- [Chat](#chat)
  - [Chat Completions](#chat-completions)
  - [Streaming](#chat-streaming)
- [Edits](#edits)
  - [Create Edit](#create-edit)
- [Embeddings](#embeddings)
  - [Create Embedding](#create-embeddings)
- [Audio](#audio)
  - [Create Transcription](#create-transcription)
  - [Create Translation](#create-translation)
- [Images](#images)
  - [Create Image](#create-image)
  - [Edit Image](#edit-image)
  - [Create Image Variation](#create-image-variation)
- [Files](#files)
  - [List Files](#list-files)
  - [Upload File](#upload-file)
  - [Delete File](#delete-file)
  - [Retrieve File Info](#retrieve-file-info)
  - [Download File Content](#download-file-content)
- [Fine Tuning](#fine-tuning)
  - [Create Fine Tune Job](#create-fine-tune-job)
  - [List Fine Tune Jobs](#list-fine-tune-jobs)
  - [Retrieve Fine Tune Job Info](#retrieve-fine-tune-job-info)
  - [Cancel Fine Tune Job](#cancel-fine-tune-job)
  - [List Fine Tune Events](#list-fine-tune-events)
  - [Stream Fine Tune Events](#stream-fine-tune-events)
- [Moderations](#moderations)
  - [Create Moderation](#create-moderation)

### Authentication

There are 3 ways to provide your API keys, in order of precedence:

1. [Pass keys directly with constructor](#pass-keys-directly-with-constructor)
2. [Load key from configuration file](#load-key-from-configuration-file)
3. [Use System Environment Variables](#use-system-environment-variables)

You use the `OpenAIAuthentication` when you initialize the API as shown:

#### Pass keys directly with constructor

```csharp
var api = new OpenAIClient("sk-apiKey");
```

Or create a `OpenAIAuthentication` object manually

```csharp
var api = new OpenAIClient(new OpenAIAuthentication("sk-apiKey", "org-yourOrganizationId"));
```

#### Load key from configuration file

Attempts to load api keys from a configuration file, by default `.openai` in the current directory, optionally traversing up the directory tree or in the user's home directory.

To create a configuration file, create a new text file named `.openai` and containing the line:

> Organization entry is optional.

##### Json format

```json
{
  "apiKey": "sk-aaaabbbbbccccddddd",
  "organization": "org-yourOrganizationId"
}
```

##### Deprecated format

```shell
OPENAI_KEY=sk-aaaabbbbbccccddddd
ORGANIZATION=org-yourOrganizationId
```

You can also load the file directly with known path by calling a static method in Authentication:

```csharp
var api = new OpenAIClient(OpenAIAuthentication.LoadFromDirectory("your/path/to/.openai"));;
```

#### Use System Environment Variables

Use your system's environment variables specify an api key and organization to use.

- Use `OPENAI_API_KEY` for your api key.
- Use `OPENAI_ORGANIZATION_ID` to specify an organization.

```csharp
var api = new OpenAIClient(OpenAIAuthentication.LoadFromEnv());
```

### [Azure OpenAI](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/)

You can also choose to use Microsoft's Azure OpenAI deployments as well.
To setup the client to use your deployment, you'll need to pass in `OpenAIClientSettings` into the client constructor.

```csharp
var auth = new OpenAIAuthentication("sk-apiKey");
var settings = new OpenAIClientSettings(resourceName: "your-resource", deploymentId: "your-deployment-id");
var api = new OpenAIClient(auth, settings);
```

### [OpenAI API Proxy](OpenAI-DotNet-Proxy/Readme.md)

[![NuGet version (OpenAI-DotNet-Proxy)](https://img.shields.io/nuget/v/OpenAI-DotNet-Proxy.svg?label=OpenAI-DotNet-Proxy&logo=nuget)](https://www.nuget.org/packages/OpenAI-DotNet-Proxy/)

Using either the [OpenAI-DotNet](https://github.com/RageAgainstThePixel/OpenAI-DotNet) or [com.openai.unity](https://github.com/RageAgainstThePixel/com.openai.unity) packages directly in your front-end app may expose your API keys and other sensitive information. To mitigate this risk, it is recommended to set up an intermediate API that makes requests to OpenAI on behalf of your front-end app. This library can be utilized for both front-end and intermediary host configurations, ensuring secure communication with the OpenAI API.

#### Front End Example

In the front end example, you will need to securely authenticate your users using your preferred OAuth provider. Once the user is authenticated, exchange your custom auth token with your API key on the backend.

Follow these steps:

1. Setup a new project using either the [OpenAI-DotNet](https://github.com/RageAgainstThePixel/OpenAI-DotNet) or [com.openai.unity](https://github.com/RageAgainstThePixel/com.openai.unity) packages.
2. Authenticate users with your OAuth provider.
3. After successful authentication, create a new `OpenAIAuthentication` object and pass in the custom token with the prefix `sess-`.
4. Create a new `OpenAIClientSettings` object and specify the domain where your intermediate API is located.
5. Pass your new `auth` and `settings` objects to the `OpenAIClient` constructor when you create the client instance.

Here's an example of how to set up the front end:

```csharp
var authToken = await LoginAsync();
var auth = new OpenAIAuthentication($"sess-{authToken}");
var settings = new OpenAIClientSettings(domain: "api.your-custom-domain.com");
var api = new OpenAIClient(auth, settings);
```

This setup allows your front end application to securely communicate with your backend that will be using the OpenAI-DotNet-Proxy, which then forwards requests to the OpenAI API. This ensures that your OpenAI API keys and other sensitive information remain secure throughout the process.

#### Back End Example

In this example, we demonstrate how to set up and use `OpenAIProxyStartup` in a new ASP.NET Core web app. The proxy server will handle authentication and forward requests to the OpenAI API, ensuring that your API keys and other sensitive information remain secure.

1. Create a new [ASP.NET Core minimal web API](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-6.0) project.
2. Add the OpenAI-DotNet nuget package to your project.
    - Powershell install: `Install-Package OpenAI-DotNet-Proxy`
    - Manually editing .csproj: `<PackageReference Include="OpenAI-DotNet-Proxy" />`
3. Create a new class that inherits from `AbstractAuthenticationFilter` and override the `ValidateAuthentication` method. This will implement the `IAuthenticationFilter` that you will use to check user session token against your internal server.
4. In `Program.cs`, create a new proxy web application by calling `OpenAIProxyStartup.CreateDefaultHost` method, passing your custom `AuthenticationFilter` as a type argument.
5. Create `OpenAIAuthentication` and `OpenAIClientSettings` as you would normally with your API keys, org id, or Azure settings.

```csharp
public partial class Program
{
    private class AuthenticationFilter : AbstractAuthenticationFilter
    {
        public override void ValidateAuthentication(IHeaderDictionary request)
        {
            // You will need to implement your own class to properly test
            // custom issued tokens you've setup for your end users.
            if (!request.Authorization.ToString().Contains(userToken))
            {
                throw new AuthenticationException("User is not authorized");
            }
        }
    }

    public static void Main(string[] args)
    {
        var auth = OpenAIAuthentication.LoadFromEnv();
        var settings = new OpenAIClientSettings(/* your custom settings if using Azure OpenAI */);
        var openAIClient = new OpenAIClient(auth, settings);
        var proxy = OpenAIProxyStartup.CreateDefaultHost<AuthenticationFilter>(args, openAIClient);
        proxy.Run();
    }
}
```

Once you have set up your proxy server, your end users can now make authenticated requests to your proxy api instead of directly to the OpenAI API. The proxy server will handle authentication and forward requests to the OpenAI API, ensuring that your API keys and other sensitive information remain secure.

### [Models](https://beta.openai.com/docs/api-reference/models)

List and describe the various models available in the API. You can refer to the [Models documentation](https://beta.openai.com/docs/models) to understand what models are available and the differences between them.

Also checkout [model endpoint compatibility](https://platform.openai.com/docs/models/model-endpoint-compatibility) to understand which models work with which endpoints.

The Models API is accessed via `OpenAIClient.ModelsEndpoint`

#### [List models](https://beta.openai.com/docs/api-reference/models/list)

Lists the currently available models, and provides basic information about each one such as the owner and availability.

```csharp
var api = new OpenAIClient();
var models = await api.ModelsEndpoint.GetModelsAsync();

foreach (var model in models)
{
    Console.WriteLine(model.ToString());
}
```

#### [Retrieve model](https://beta.openai.com/docs/api-reference/models/retrieve)

Retrieves a model instance, providing basic information about the model such as the owner and permissions.

```csharp
var api = new OpenAIClient();
var model = await api.ModelsEndpoint.GetModelDetailsAsync("text-davinci-003");
Console.WriteLine(model.ToString());
```

#### [Delete Fine Tuned Model](https://beta.openai.com/docs/api-reference/fine-tunes/delete-model)

Delete a fine-tuned model. You must have the Owner role in your organization.

```csharp
var api = new OpenAIClient();
var result = await api.ModelsEndpoint.DeleteFineTuneModelAsync("your-fine-tuned-model");
Assert.IsTrue(result);
```

### [Completions](https://beta.openai.com/docs/api-reference/completions)

Given a prompt, the model will return one or more predicted completions, and can also return the probabilities of alternative tokens at each position.

The Completions API is accessed via `OpenAIClient.CompletionsEndpoint`

```csharp
var api = new OpenAIClient();
var result = await api.CompletionsEndpoint.CreateCompletionAsync("One Two Three One Two", temperature: 0.1, model: Model.Davinci);
Console.WriteLine(result);
```

> To get the `CompletionResult` (which is mostly metadata), use its implicit string operator to get the text if all you want is the completion choice.

#### Completion Streaming

Streaming allows you to get results are they are generated, which can help your application feel more responsive, especially on slow models like Davinci.

```csharp
var api = new OpenAIClient();

await api.CompletionsEndpoint.StreamCompletionAsync(result =>
{
    foreach (var choice in result.Completions)
    {
        Console.WriteLine(choice);
    }
}, "My name is Roger and I am a principal software engineer at Salesforce.  This is my resume:", maxTokens: 200, temperature: 0.5, presencePenalty: 0.1, frequencyPenalty: 0.1, model: Model.Davinci);
```

Or if using [`IAsyncEnumerable{T}`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1?view=net-5.0) ([C# 8.0+](https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8))

```csharp
var api = new OpenAIClient();
await foreach (var token in api.CompletionsEndpoint.StreamCompletionEnumerableAsync("My name is Roger and I am a principal software engineer at Salesforce.  This is my resume:", maxTokens: 200, temperature: 0.5, presencePenalty: 0.1, frequencyPenalty: 0.1, model: Model.Davinci))
{
  Console.WriteLine(token);
}
```

### [Chat](https://platform.openai.com/docs/api-reference/chat)

Given a chat conversation, the model will return a chat completion response.

The Chat API is accessed via `OpenAIClient.ChatEndpoint`

#### [Chat Completions](https://platform.openai.com/docs/api-reference/chat/create)

Creates a completion for the chat message

```csharp
var api = new OpenAIClient();
var chatPrompts = new List<ChatPrompt>
{
    new ChatPrompt("system", "You are a helpful assistant."),
    new ChatPrompt("user", "Who won the world series in 2020?"),
    new ChatPrompt("assistant", "The Los Angeles Dodgers won the World Series in 2020."),
    new ChatPrompt("user", "Where was it played?"),
};
var chatRequest = new ChatRequest(chatPrompts);
var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
Console.WriteLine(result.FirstChoice);
```

##### [Chat Streaming](https://platform.openai.com/docs/api-reference/chat/create#chat/create-stream)

```csharp
var api = new OpenAIClient();
var chatPrompts = new List<ChatPrompt>
{
    new ChatPrompt("system", "You are a helpful assistant."),
    new ChatPrompt("user", "Who won the world series in 2020?"),
    new ChatPrompt("assistant", "The Los Angeles Dodgers won the World Series in 2020."),
    new ChatPrompt("user", "Where was it played?"),
};
var chatRequest = new ChatRequest(chatPrompts, Model.GPT3_5_Turbo);

await api.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
{
    Console.WriteLine(result.FirstChoice);
});
```

Or if using [`IAsyncEnumerable{T}`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1?view=net-5.0) ([C# 8.0+](https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8))

```csharp
var api = new OpenAIClient();
var chatPrompts = new List<ChatPrompt>
{
    new ChatPrompt("system", "You are a helpful assistant."),
    new ChatPrompt("user", "Who won the world series in 2020?"),
    new ChatPrompt("assistant", "The Los Angeles Dodgers won the World Series in 2020."),
    new ChatPrompt("user", "Where was it played?"),
};
var chatRequest = new ChatRequest(chatPrompts, Model.GPT3_5_Turbo);

await foreach (var result in api.ChatEndpoint.StreamCompletionEnumerableAsync(chatRequest))
{
    Console.WriteLine(result.FirstChoice);
}
```

### [Edits](https://beta.openai.com/docs/api-reference/edits)

Given a prompt and an instruction, the model will return an edited version of the prompt.

The Edits API is accessed via `OpenAIClient.EditsEndpoint`

#### [Create Edit](https://beta.openai.com/docs/api-reference/edits/create)

Creates a new edit for the provided input, instruction, and parameters using the provided input and instruction.

```csharp
var api = new OpenAIClient();
var request = new EditRequest("What day of the wek is it?", "Fix the spelling mistakes");
var result = await api.EditsEndpoint.CreateEditAsync(request);
Console.WriteLine(result);
```

### [Embeddings](https://beta.openai.com/docs/api-reference/embeddings)

Get a vector representation of a given input that can be easily consumed by machine learning models and algorithms.

Related guide: [Embeddings](https://beta.openai.com/docs/guides/embeddings)

The Edits API is accessed via `OpenAIClient.EmbeddingsEndpoint`

#### [Create Embeddings](https://beta.openai.com/docs/api-reference/embeddings/create)

Creates an embedding vector representing the input text.

```csharp
var api = new OpenAIClient();
var model = await api.ModelsEndpoint.GetModelDetailsAsync("text-embedding-ada-002");
var result = await api.EmbeddingsEndpoint.CreateEmbeddingAsync("The food was delicious and the waiter...", model);
Console.WriteLine(result);
```

### [Audio](https://beta.openai.com/docs/api-reference/audio)

Converts audio into text.

The Audio API is accessed via `OpenAIClient.AudioEndpoint`

#### [Create Transcription](https://platform.openai.com/docs/api-reference/audio/create)

Transcribes audio into the input language.

```csharp
var api = new OpenAIClient();
var request = new AudioTranscriptionRequest(Path.GetFullPath(audioAssetPath), language: "en");
var result = await api.AudioEndpoint.CreateTranscriptionAsync(request);
Console.WriteLine(result);
```

#### [Create Translation](https://platform.openai.com/docs/api-reference/audio/create)

Translates audio into into English.

```csharp
var api = new OpenAIClient();
var request = new AudioTranslationRequest(Path.GetFullPath(audioAssetPath));
var result = await api.AudioEndpoint.CreateTranslationAsync(request);
Console.WriteLine(result);
```

### [Images](https://beta.openai.com/docs/api-reference/images)

Given a prompt and/or an input image, the model will generate a new image.

The Images API is accessed via `OpenAIClient.ImagesEndpoint`

#### [Create Image](https://beta.openai.com/docs/api-reference/images/create)

Creates an image given a prompt.

```csharp
var api = new OpenAIClient();
var results = await api.ImagesEndPoint.GenerateImageAsync("A house riding a velociraptor", 1, ImageSize.Small);

foreach (var result in results)
{
    Console.WriteLine(result);
    // result == file://path/to/image.png
}
```

#### [Edit Image](https://beta.openai.com/docs/api-reference/images/create-edit)

Creates an edited or extended image given an original image and a prompt.

```csharp
var api = new OpenAIClient();
var results = await api.ImagesEndPoint.CreateImageEditAsync(Path.GetFullPath(imageAssetPath), Path.GetFullPath(maskAssetPath), "A sunlit indoor lounge area with a pool containing a flamingo", 1, ImageSize.Small);

foreach (var result in results)
{
    Console.WriteLine(result);
    // result == file://path/to/image.png
}
```

#### [Create Image Variation](https://beta.openai.com/docs/api-reference/images/create-variation)

Creates a variation of a given image.

```csharp
var api = new OpenAIClient();
var results = await api.ImagesEndPoint.CreateImageVariationAsync(Path.GetFullPath(imageAssetPath), 1, ImageSize.Small);

foreach (var result in results)
{
    Console.WriteLine(result);
    // result == file://path/to/image.png
}
```

### [Files](https://beta.openai.com/docs/api-reference/files)

Files are used to upload documents that can be used with features like [Fine-tuning](#fine-tuning).

The Files API is accessed via `OpenAIClient.FilesEndpoint`

#### [List Files](https://beta.openai.com/docs/api-reference/files/list)

Returns a list of files that belong to the user's organization.

```csharp
var api = new OpenAIClient();
var files = await api.FilesEndpoint.ListFilesAsync();

foreach (var file in files)
{
    Console.WriteLine($"{file.Id} -> {file.Object}: {file.FileName} | {file.Size} bytes");
}
```

#### [Upload File](https://beta.openai.com/docs/api-reference/files/upload)

Upload a file that contains document(s) to be used across various endpoints/features. Currently, the size of all the files uploaded by one organization can be up to 1 GB. Please contact us if you need to increase the storage limit.

```csharp
var api = new OpenAIClient();
var fileData = await api.FilesEndpoint.UploadFileAsync("path/to/your/file.jsonl", "fine-tune");
Console.WriteLine(fileData.Id);
```

#### [Delete File](https://beta.openai.com/docs/api-reference/files/delete)

Delete a file.

```csharp
var api = new OpenAIClient();
var result = await api.FilesEndpoint.DeleteFileAsync(fileData);
Assert.IsTrue(result);
```

#### [Retrieve File Info](https://beta.openai.com/docs/api-reference/files/retrieve)

Returns information about a specific file.

```csharp
var api = new OpenAIClient();
var fileData = await GetFileInfoAsync(fileId);
Console.WriteLine($"{fileData.Id} -> {fileData.Object}: {fileData.FileName} | {fileData.Size} bytes");
```

#### [Download File Content](https://beta.openai.com/docs/api-reference/files/retrieve-content)

Downloads the specified file.

```csharp
var api = new OpenAIClient();
var downloadedFilePath = await api.FilesEndpoint.DownloadFileAsync(fileId, "path/to/your/save/directory");
Console.WriteLine(downloadedFilePath);
Assert.IsTrue(File.Exists(downloadedFilePath));
```

### [Fine Tuning](https://beta.openai.com/docs/api-reference/fine-tunes)

Manage fine-tuning jobs to tailor a model to your specific training data.

Related guide: [Fine-tune models](https://beta.openai.com/docs/guides/fine-tuning)

The Files API is accessed via `OpenAIClient.FineTuningEndpoint`

#### [Create Fine Tune Job](https://beta.openai.com/docs/api-reference/fine-tunes/create)

Creates a job that fine-tunes a specified model from a given dataset.

Response includes details of the enqueued job including job status and the name of the fine-tuned models once complete.

```csharp
var api = new OpenAIClient();
var request = new CreateFineTuneRequest(fileData);
var fineTuneJob = await api.FineTuningEndpoint.CreateFineTuneJobAsync(request);
Console.WriteLine(fineTuneJob.Id);
```

#### [List Fine Tune Jobs](https://beta.openai.com/docs/api-reference/fine-tunes/list)

List your organization's fine-tuning jobs.

```csharp
var api = new OpenAIClient();
var fineTuneJobs = await api.FineTuningEndpoint.ListFineTuneJobsAsync();

foreach (var job in fineTuneJobs)
{
    Console.WriteLine($"{job.Id} -> {job.Status}");
}
```

#### [Retrieve Fine Tune Job Info](https://beta.openai.com/docs/api-reference/fine-tunes/retrieve)

Gets info about the fine-tune job.

```csharp
var api = new OpenAIClient();
var result = await api.FineTuningEndpoint.RetrieveFineTuneJobInfoAsync(fineTuneJob);
Console.WriteLine($"{result.Id} -> {result.Status}");
```

#### [Cancel Fine Tune Job](https://beta.openai.com/docs/api-reference/fine-tunes/cancel)

Immediately cancel a fine-tune job.

```csharp
var api = new OpenAIClient();
var result = await api.FineTuningEndpoint.CancelFineTuneJobAsync(fineTuneJob);
Assert.IsTrue(result);
```

#### [List Fine Tune Events](https://beta.openai.com/docs/api-reference/fine-tunes/events)

Get fine-grained status updates for a fine-tune job.

```csharp
var api = new OpenAIClient();
var fineTuneEvents = await api.FineTuningEndpoint.ListFineTuneEventsAsync(fineTuneJob);
Console.WriteLine($"{fineTuneJob.Id} -> status: {fineTuneJob.Status} | event count: {fineTuneEvents.Count}");
```

#### [Stream Fine Tune Events](https://beta.openai.com/docs/api-reference/fine-tunes/events#fine-tunes/events-stream)

```csharp
var api = new OpenAIClient();
await api.FineTuningEndpoint.StreamFineTuneEventsAsync(fineTuneJob, fineTuneEvent =>
{
    Console.WriteLine($"  {fineTuneEvent.CreatedAt} [{fineTuneEvent.Level}] {fineTuneEvent.Message}");
});
```

Or if using [`IAsyncEnumerable{T}`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1?view=net-5.0) ([C# 8.0+](https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8))

```csharp
var api = new OpenAIClient();
await foreach (var fineTuneEvent in api.FineTuningEndpoint.StreamFineTuneEventsEnumerableAsync(fineTuneJob))
{
    Console.WriteLine($"  {fineTuneEvent.CreatedAt} [{fineTuneEvent.Level}] {fineTuneEvent.Message}");
}
```

### [Moderations](https://beta.openai.com/docs/api-reference/moderations)

Given a input text, outputs if the model classifies it as violating OpenAI's content policy.

Related guide: [Moderations](https://beta.openai.com/docs/guides/moderation)

The Moderations API can be accessed via `OpenAIClient.ModerationsEndpoint`

#### [Create Moderation](https://beta.openai.com/docs/api-reference/moderations/create)

Classifies if text violates OpenAI's Content Policy.

```csharp
var api = new OpenAIClient();
var response = await api.ModerationsEndpoint.GetModerationAsync("I want to kill them.");
Assert.IsTrue(response);
```

---

## License

![CC-0 Public Domain](https://licensebuttons.net/p/zero/1.0/88x31.png)

This library is licensed CC-0, in the public domain.  You can use it for whatever you want, publicly or privately, without worrying about permission or licensing or whatever.  It's just a wrapper around the OpenAI API, so you still need to get access to OpenAI from them directly.  I am not affiliated with OpenAI and this library is not endorsed by them, I just have beta access and wanted to make a C# library to access it more easily.  Hopefully others find this useful as well.  Feel free to open a PR if there's anything you want to contribute.
