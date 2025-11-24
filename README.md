# MinimalRoutes

Sistema de visualização e comparação de algoritmos para resolução do Problema do Caixeiro Viajante (TSP - Travelling Salesman Problem).

## Sobre o Projeto

Este projeto foi desenvolvido para a disciplina de Estrutura de Dados Não Lineares do curso de Análise e Desenvolvimento de Sistemas. O objetivo é implementar e comparar diferentes algoritmos de busca aplicados ao problema do ciclo hamiltoniano em grafos ponderados.

## Funcionalidades

- Visualização interativa de grafos com layout force-directed (Fruchterman-Reingold)
- Implementação de dois algoritmos de busca:
  - **Brute Force**: Busca exaustiva de todas as permutações possíveis
  - **Dijkstra adaptado para TSP**: Utiliza Priority Queue com Branch and Bound e heurísticas de poda
- Comparação de desempenho entre algoritmos
- Visualização em tempo real do caminho sendo explorado
- Modo de debug com geração de logs detalhados
- Estatísticas de execução (tempo, caminhos parciais explorados, ciclos completos)

## Tecnologias Utilizadas

- **C# (.NET 9.0)** - Linguagem de programação
- **MonoGame** - Framework para desenvolvimento de jogos e aplicações gráficas multiplataforma

## Algoritmos Implementados

### Brute Force
- Explora todas as permutações possíveis de caminhos
- Complexidade: O(n!)
- Adequado apenas para grafos pequenos (até ~10 nós)

### Dijkstra com Priority Queue (Adaptado para TSP)
- Utiliza fila de prioridade para explorar caminhos mais promissores primeiro
- Implementa técnicas de poda:
  - Branch and Bound
  - Memoização de estados
  - Heurística de lower bound (estimativa de custo restante)
- Reduz drasticamente o número de estados explorados

## Requisitos

- .NET SDK 9.0 ou superior
- Sistema operacional compatível:
  - Windows 10/11
  - Linux (distribuições modernas com suporte a .NET)

## Compilação e Execução

```bash
# Clonar o repositório
git clone https://github.com/eudivanmelo/MinimalRoutes
cd MinimalRoutes

# Compilar o projeto
dotnet build

# Executar (modo normal)
dotnet run

# Executar com logs de debug
dotnet run --debug
```

## Modo Debug

Ao executar com `--debug`, o sistema gera arquivos de log detalhados:

- `bruteforce_debug.txt` - Log da execução do algoritmo Brute Force
- `dijkstra_debug.txt` - Log da execução do algoritmo Dijkstra

Os logs contêm:
- Timestamp de execução
- Todos os caminhos parciais explorados
- Decisões de poda
- Soluções encontradas
- Estatísticas finais

## Uso da Aplicação

1. **Trocar Grafo**: Alterna entre grafo simples (5 nós) e complexo (18 nós)
2. **Força Bruta**: Executa o algoritmo de busca exaustiva
3. **Dijkstra TSP**: Executa o algoritmo otimizado com Priority Queue

Durante a execução:
- O caminho atual sendo explorado é destacado em tempo real
- Estatísticas são atualizadas continuamente
- A interface mostra:
  - Número de caminhos parciais explorados
  - Número de ciclos completos encontrados
  - Melhor distância encontrada até o momento
  - Tempo de execução

## Comparação de Performance

Exemplo de resultados em grafo complexo (18 nós):

| Algoritmo | Caminhos Parciais | Tempo |
|-----------|------------------|-------|
| Brute Force | 1.833.869 | ~0,69s |
| Dijkstra TSP | 90.660 | ~0,41s |

## Licença

Este projeto foi desenvolvido para fins educacionais.