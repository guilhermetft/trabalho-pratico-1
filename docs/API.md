# Documentação da API - Locadora de Veículos

Base URL (desenvolvimento): `http://localhost:5215`. Swagger UI interativo: `http://localhost:5215/swagger` (JSON da especificação OpenAPI em `/swagger/v1/swagger.json`).

Todos os endpoints consomem e produzem `application/json`. Enums trafegam como string (`StatusVeiculo`: `Disponivel`, `Alugado`, `Manutencao`). Datas em ISO 8601 (`2026-10-05T14:30:00`).

## Formato de erros

| Código | Quando | Corpo |
|---|---|---|
| `400 Bad Request` | Falha de validação do corpo/parâmetros, ou referência a entidade inexistente no corpo (ex.: `fabricanteId` que não existe) | Validação: `ProblemDetails` com `errors` por campo. Demais: `{ "mensagem": "..." }` |
| `404 Not Found` | Recurso do Id informado não existe | `{ "mensagem": "Fabricante 9999 não encontrado." }` |
| `409 Conflict` | Violação de unicidade, de FK ou de regra de negócio | `{ "mensagem": "..." }` |
| `500 Internal Server Error` | Erro não tratado | `ProblemDetails` (`title`: "Erro interno ao processar a requisição.") |

## Resumo dos endpoints

| Recurso | Método e rota | Sucesso | Erros possíveis |
|---|---|---|---|
| Fabricantes | `GET /api/fabricantes` | 200 | 500 |
| | `GET /api/fabricantes/{id}` | 200 | 404 |
| | `POST /api/fabricantes` | 201 | 400, 409 |
| | `PUT /api/fabricantes/{id}` | 204 | 400, 404, 409 |
| | `DELETE /api/fabricantes/{id}` | 204 | 404, 409 |
| Categorias | `GET /api/categorias` | 200 | 500 |
| | `GET /api/categorias/{id}` | 200 | 404 |
| | `POST /api/categorias` | 201 | 400, 409 |
| | `PUT /api/categorias/{id}` | 204 | 400, 404, 409 |
| | `DELETE /api/categorias/{id}` | 204 | 404, 409 |
| Clientes | `GET /api/clientes` | 200 | 500 |
| | `GET /api/clientes/{id}` | 200 | 404 |
| | `GET /api/clientes/sem-alugueis` (filtro 5) | 200 | 500 |
| | `POST /api/clientes` | 201 | 400, 409 |
| | `PUT /api/clientes/{id}` | 204 | 400, 404, 409 |
| | `DELETE /api/clientes/{id}` | 204 | 404, 409 |
| Veículos | `GET /api/veiculos` | 200 | 500 |
| | `GET /api/veiculos/{id}` | 200 | 404 |
| | `GET /api/veiculos/disponiveis` (filtro 1) | 200 | 500 |
| | `GET /api/veiculos/por-fabricante/{fabricanteId}` (filtro 2) | 200 | 404 |
| | `POST /api/veiculos` | 201 | 400, 409 |
| | `PUT /api/veiculos/{id}` | 204 | 400, 404, 409 |
| | `DELETE /api/veiculos/{id}` | 204 | 404, 409 |
| Aluguéis | `GET /api/alugueis` | 200 | 500 |
| | `GET /api/alugueis/{id}` | 200 | 404 |
| | `GET /api/alugueis/por-cliente/{clienteId}` (filtro 3) | 200 | 404 |
| | `GET /api/alugueis/atrasados` (filtro 4) | 200 | 500 |
| | `POST /api/alugueis` | 201 | 400, 409 |
| | `PUT /api/alugueis/{id}` | 204 | 400, 404, 409 |
| | `POST /api/alugueis/{id}/devolucao` | 200 | 400, 404, 409 |
| | `DELETE /api/alugueis/{id}` | 204 | 404, 409 |

Parâmetros de rota `{id}`, `{fabricanteId}` e `{clienteId}` são inteiros (`int32`).

---

## Fabricantes

Corpo de `POST` e `PUT`:

| Campo | Tipo | Regras |
|---|---|---|
| `nome` | string | obrigatório, 2 a 100 caracteres, único |
| `paisOrigem` | string | obrigatório, 2 a 60 caracteres |

- `GET` (lista/id) retorna `{ id, nome, paisOrigem }`.
- `409` no `POST`/`PUT`: nome já existe. `409` no `DELETE`: fabricante possui veículos vinculados.

## Categorias

| Campo | Tipo | Regras |
|---|---|---|
| `nome` | string | obrigatório, 2 a 60 caracteres, único |
| `descricao` | string | opcional, até 255 caracteres |
| `valorDiariaBase` | decimal | entre 0,01 e 100000 |

- `GET` retorna `{ id, nome, descricao, valorDiariaBase }`.
- `409`: nome duplicado (`POST`/`PUT`) ou categoria com veículos vinculados (`DELETE`).

## Clientes

| Campo | Tipo | Regras |
|---|---|---|
| `nome` | string | obrigatório, 2 a 150 caracteres |
| `cpf` | string | obrigatório, exatamente 11 dígitos, único |
| `email` | string | obrigatório, formato de e-mail, até 150 caracteres, único |
| `telefone` | string | opcional, até 20 caracteres |
| `cnh` | string | opcional, até 11 caracteres |

- `GET` retorna `{ id, nome, cpf, email, telefone, cnh }`.
- `GET /api/clientes/sem-alugueis` (filtro 5): clientes que nunca alugaram (LEFT JOIN Clientes x Alugueis).
- `409`: CPF ou e-mail duplicado (`POST`/`PUT`) ou cliente com aluguéis (`DELETE`).

## Veículos

| Campo | Tipo | Regras |
|---|---|---|
| `modelo` | string | obrigatório, 2 a 100 caracteres |
| `placa` | string | obrigatório, padrão `AAA0000` ou `AAA0A00` (sem hífen), única |
| `anoFabricacao` | int | 1900 a 2100 |
| `quilometragem` | int | >= 0 |
| `status` | string | `Disponivel`, `Alugado` ou `Manutencao` (padrão `Disponivel` no `POST`) |
| `fabricanteId` | int | obrigatório, deve existir |
| `categoriaId` | int | obrigatório, deve existir |

- `GET` retorna os campos acima mais `fabricanteNome`, `categoriaNome` e `valorDiariaBase`.
- `GET /api/veiculos/disponiveis?categoriaId=&fabricanteId=` (filtro 1): veículos com status `Disponivel`; os dois parâmetros de query são opcionais. INNER JOIN Veiculos x Categorias x Fabricantes.
- `GET /api/veiculos/por-fabricante/{fabricanteId}` (filtro 2): `404` se o fabricante não existe. INNER JOIN Veiculos x Fabricantes x Categorias.
- `400` no `POST`/`PUT` também quando fabricante ou categoria não existem. `409`: placa duplicada (`POST`/`PUT`) ou veículo com aluguéis (`DELETE`).

## Aluguéis

Corpo de `POST /api/alugueis`:

| Campo | Tipo | Regras |
|---|---|---|
| `clienteId` | int | obrigatório, deve existir |
| `veiculoId` | int | obrigatório, deve existir |
| `dataRetirada` | datetime | obrigatório |
| `dataPrevistaDevolucao` | datetime | obrigatório, >= `dataRetirada` |
| `valorDiaria` | decimal | entre 0,01 e 100000 |

Corpo de `PUT /api/alugueis/{id}`: `dataRetirada`, `dataPrevistaDevolucao`, `valorDiaria` (mesmas regras).

Corpo de `POST /api/alugueis/{id}/devolucao`:

| Campo | Tipo | Regras |
|---|---|---|
| `kmFinal` | int | >= 0 e >= `kmInicial` do aluguel |
| `dataDevolucao` | datetime | opcional (padrão: agora), >= `dataRetirada` |

Retorno de `GET`: `id`, `dataRetirada`, `dataPrevistaDevolucao`, `dataDevolucao`, `kmInicial`, `kmFinal`, `valorDiaria`, `valorTotal`, `clienteId`, `clienteNome`, `veiculoId`, `veiculoModelo`, `veiculoPlaca`. `dataDevolucao`, `kmFinal` e `valorTotal` são `null` até a devolução.

Regras de negócio e códigos:

- `POST`: `400` se cliente/veículo não existe ou datas incoerentes; `409` se o veículo não está `Disponivel`. `kmInicial` é preenchido com a quilometragem atual do veículo e o veículo passa a `Alugado`.
- `PUT`: `409` se o aluguel já foi devolvido.
- `POST .../devolucao`: calcula `valorTotal` = dias corridos (arredondados para cima, mínimo 1) x `valorDiaria`; atualiza a quilometragem do veículo e volta o status para `Disponivel`. `400` se `kmFinal` < `kmInicial` ou data anterior à retirada; `409` se já devolvido.
- `DELETE`: se o aluguel ainda estava ativo, o veículo é liberado (`Disponivel`).
- `GET /api/alugueis/por-cliente/{clienteId}` (filtro 3): `404` se o cliente não existe. INNER JOIN Alugueis x Clientes x Veiculos.
- `GET /api/alugueis/atrasados` (filtro 4): aluguéis sem devolução e com `dataPrevistaDevolucao` vencida. INNER JOIN Alugueis x Clientes x Veiculos.

Exemplo de `POST /api/alugueis`:

```json
{
  "clienteId": 1,
  "veiculoId": 1,
  "dataRetirada": "2026-10-05T10:00:00",
  "dataPrevistaDevolucao": "2026-10-10T10:00:00",
  "valorDiaria": 120
}
```

Os testes de todos esses endpoints, com prints do Swagger, estão em [`RELATORIO-TESTES.md`](RELATORIO-TESTES.md).
