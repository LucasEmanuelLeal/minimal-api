# Minimal API - Sistema de Registro de Veículos

API REST desenvolvida com .NET 7 utilizando Minimal APIs para registro e gerenciamento de veículos, com autenticação JWT e controle de acesso por perfis de administrador.

## 📋 Descrição

Esta API permite o gerenciamento de veículos e administradores, com as seguintes funcionalidades:

- **Autenticação JWT**: Sistema de login com geração de tokens JWT
- **Controle de Acesso**: Perfis de Administrador (Adm) e Editor
- **CRUD de Veículos**: Criar, ler, atualizar e deletar veículos
- **CRUD de Administradores**: Gerenciar administradores do sistema
- **Validações**: Validação de dados nas operações de criação e atualização
- **Swagger**: Documentação interativa da API
- **Testes**: Testes unitários e de integração

## 🚀 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [MySQL](https://www.mysql.com/downloads/) (versão 8.0 ou superior)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/) (opcional)

## 📦 Instalação

1. Clone o repositório (ou extraia os arquivos):

```bash
git clone <url-do-repositorio>
cd minimal-api
```

2. Configure a conexão com o banco de dados no arquivo `Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Database=minimal_api;Uid=root;Pwd=sua_senha;"
  },
  "Jwt": "sua-chave-secreta-jwt-aqui"
}
```

3. Crie o banco de dados MySQL:

```sql
CREATE DATABASE minimal_api;
```

4. Execute as migrações do Entity Framework:

```bash
cd Api
dotnet ef database update
```

## 🔧 Configuração

### Configuração do Banco de Dados

Edite o arquivo `Api/appsettings.json` e ajuste a string de conexão:

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Database=minimal_api;Uid=seu_usuario;Pwd=sua_senha;"
  }
}
```

### Configuração do JWT

A chave JWT está configurada no arquivo `appsettings.json`:

```json
{
  "Jwt": "sua-chave-secreta-jwt-aqui"
}
```

⚠️ **Importante**: Em produção, use uma chave JWT mais segura e mantenha-a em variáveis de ambiente.

## 🏃 Executando a Aplicação

1. Navegue até a pasta do projeto API:

```bash
cd Api
```

2. Execute a aplicação:

```bash
dotnet run
```

3. Acesse a documentação Swagger:

```
http://localhost:5004/swagger
```

Ou use o perfil HTTPS:

```
https://localhost:7257/swagger
```

## 🧪 Executando os Testes

1. Execute todos os testes:

```bash
dotnet test
```

2. Execute testes específicos:

```bash
dotnet test --filter "FullyQualifiedName~AdministradorTest"
```

### Configuração do Banco para Testes

Os testes usam um banco de dados separado. Configure em `Test/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Database=minimal_apitest;Uid=seu_usuario;Pwd=sua_senha;"
  },
  "Jwt": "sua-chave-secreta-jwt-aqui"
}
```

**Nota**: Crie o banco de dados de teste antes de executar os testes:

```sql
CREATE DATABASE minimal_apitest;
```

## 📚 Endpoints da API

### Home

- **GET** `/` - Mensagem de boas-vindas (público)

### Autenticação

- **POST** `/administradores/login` - Realizar login e obter token JWT (público)
  ```json
  {
    "email": "administrador@teste.com",
    "senha": "123456"
  }
  ```

### Administradores

- **GET** `/administradores` - Listar administradores (requer autenticação + perfil Adm)
  - Query params: `pagina` (opcional)
- **GET** `/administradores/{id}` - Obter administrador por ID (requer autenticação + perfil Adm)
- **POST** `/administradores` - Criar novo administrador (requer autenticação + perfil Adm)
  ```json
  {
    "email": "novo@teste.com",
    "senha": "senha123",
    "perfil": "Editor"
  }
  ```

### Veículos

- **GET** `/veiculos` - Listar veículos (requer autenticação)
  - Query params: `pagina`, `nome`, `marca` (todos opcionais)
- **GET** `/veiculos/{id}` - Obter veículo por ID (requer autenticação + perfil Adm ou Editor)
- **POST** `/veiculos` - Criar novo veículo (requer autenticação + perfil Adm ou Editor)
  ```json
  {
    "nome": "Fiesta 2.0",
    "marca": "Ford",
    "ano": 2013
  }
  ```
- **PUT** `/veiculos/{id}` - Atualizar veículo (requer autenticação + perfil Adm)
- **DELETE** `/veiculos/{id}` - Deletar veículo (requer autenticação + perfil Adm)

## 🔐 Autenticação JWT

### Como obter o token

1. Faça uma requisição POST para `/administradores/login` com email e senha:

```bash
POST /administradores/login
Content-Type: application/json

{
  "email": "administrador@teste.com",
  "senha": "123456"
}
```

2. A resposta incluirá o token JWT:

```json
{
  "email": "administrador@teste.com",
  "perfil": "Adm",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Como usar o token

Inclua o token no header `Authorization` de todas as requisições autenticadas:

```
Authorization: Bearer <seu-token-jwt>
```

### Usando no Swagger

1. Faça login em `/administradores/login`
2. Copie o token retornado
3. Clique no botão **Authorize** no Swagger
4. Cole o token no formato: `Bearer <token>`
5. Clique em **Authorize** e depois em **Close**

## 👥 Perfis de Acesso

- **Adm**: Acesso total a todas as funcionalidades
- **Editor**: Pode visualizar e criar veículos, mas não pode atualizar ou deletar

## 📁 Estrutura do Projeto

```
minimal-api/
├── Api/                          # Projeto principal da API
│   ├── Dominio/                  # Camada de domínio
│   │   ├── DTOs/                 # Data Transfer Objects
│   │   ├── Entidades/            # Entidades do domínio
│   │   ├── Enuns/                # Enumeradores
│   │   ├── Interfaces/           # Interfaces dos serviços
│   │   ├── ModelViews/           # Modelos de visualização
│   │   └── Servicos/             # Serviços de negócio
│   ├── Infraestrutura/           # Camada de infraestrutura
│   │   └── Db/                   # Contexto do Entity Framework
│   ├── Migrations/               # Migrações do banco de dados
│   ├── Program.cs                # Ponto de entrada da aplicação
│   └── Startup.cs                # Configuração da aplicação
├── Test/                         # Projeto de testes
│   ├── Domain/                   # Testes de domínio
│   ├── Helpers/                  # Helpers para testes
│   ├── Mocks/                    # Mocks para testes
│   └── Requests/                 # Testes de requisições HTTP
└── minimal-api.sln               # Solução do Visual Studio
```

## 🗄️ Banco de Dados

### Administrador Padrão

Após executar as migrações, um administrador padrão será criado:

- **Email**: `administrador@teste.com`
- **Senha**: `123456`
- **Perfil**: `Adm`

### Tabelas

- **Administradores**: Armazena os administradores do sistema
- **Veiculos**: Armazena os veículos cadastrados

## ✅ Validações

### Veículos

- Nome não pode ser vazio
- Marca não pode ser vazia
- Ano deve ser superior a 1950

### Administradores

- Email não pode ser vazio
- Senha não pode ser vazia
- Perfil não pode ser vazio

## 🚀 Deploy

### Preparação para produção

1. Configure variáveis de ambiente para:

   - String de conexão do banco de dados
   - Chave JWT secreta
   - Ambiente (Production)

2. Publique a aplicação:

```bash
dotnet publish -c Release -o ./publish
```

3. Execute as migrações no servidor de produção:

```bash
dotnet ef database update
```

## 🧪 Testes

O projeto inclui testes de persistência e testes de integração HTTP:

- **Testes de Persistência**: Testam os serviços e acesso ao banco de dados
- **Testes de Requests**: Testam as rotas HTTP da API com autenticação JWT

Para executar os testes:

```bash
dotnet test
```

## 📝 Notas Importantes

- A senha dos administradores está sendo armazenada em texto plano. Em produção, considere usar hash (BCrypt, Argon2, etc.)
- A chave JWT está em texto plano no `appsettings.json`. Em produção, use variáveis de ambiente ou Azure Key Vault
- Configure as credenciais do banco de dados nos arquivos `appsettings.json` antes de executar a aplicação ou testes

## 🤝 Contribuindo

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto foi desenvolvido para fins educacionais.

## 👨‍💻 Autor

Desenvolvido como parte de um LAB sobre Minimal APIs e autenticação JWT.

---

Para mais informações, consulte a documentação do Swagger em `/swagger` quando a aplicação estiver em execução.
