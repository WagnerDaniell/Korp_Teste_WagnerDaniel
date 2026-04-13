# 🚀 KorpNF — Sistema de Emissão de Notas Fiscais

Este projeto foi desenvolvido como um desafio técnico para a **Korp Viasoft**. O objetivo é oferecer uma solução simples e eficiente para gerenciar o estoque de produtos e realizar a emissão de Notas Fiscais de forma organizada.

O sistema foi construído pensando na facilidade de uso e na segurança dos dados, garantindo que o estoque esteja sempre batendo com as notas emitidas.

---

### 🛠️ O que o sistema faz?

De forma simples, você consegue:

**Gerenciar Produtos:** Cadastrar seus produtos com nome, código e a quantidade que você tem em estoque.
**Criar Notas Fiscais:** Abrir novas notas e adicionar quantos produtos precisar.
**Finalizar e Baixar Estoque:** Ao "imprimir" (finalizar) uma nota, o sistema fecha o documento e retira automaticamente as quantidades do seu saldo de produtos.
**Prevenção de Erros:** O sistema avisa se você tentar vender algo que não tem no estoque ou se houver algum problema de conexão.

---

### 🐳 Como rodar o projeto?

Para facilitar a sua vida, o projeto está totalmente "containerizado". Isso significa que você não precisa instalar bancos de dados ou configurar linguagens na sua máquina. Você só precisa ter o **Docker** instalado.

**Siga os passos:**

1.  Abra o terminal (ou CMD) na pasta raiz do projeto.
2.  Execute o comando mágico:
    ```bash
    docker-compose up --build
    ```
3.  Aguarde o processo terminar (o Docker vai baixar e configurar tudo sozinho).
4.  Quando finalizar, abra o seu navegador e acesse:
    👉 **http://localhost:4200**

---

### 📝 Observações
* O sistema já sobe com o banco de dados configurado e as tabelas criadas automaticamente.
* As APIs de **Estoque** e **Faturamento** rodam em segundo plano para garantir que cada parte do sistema cuide da sua própria responsabilidade.

---
**Desenvolvido com foco em qualidade por Wagner Daniel.**