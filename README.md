# MinimalRoutes

Sistema de visualização e comparação de algoritmos para encontrar o menor caminho entre dois pontos em grafos ponderados.

## Sobre o Projeto

Este projeto foi desenvolvido para a disciplina de Estrutura de Dados Não Lineares do curso de Análise e Desenvolvimento de Sistemas. O objetivo é implementar e comparar diferentes algoritmos de busca do menor caminho entre dois nós em grafos ponderados.

## Funcionalidades

- Visualização interativa de grafos com layout force-directed (Fruchterman-Reingold)
- Seleção de nós inicial e final através de cliques
- Implementação de dois algoritmos de busca:
  - **Brute Force**: Busca exaustiva de todos os caminhos possíveis entre dois nós
  - **Dijkstra**: Algoritmo clássico de menor caminho com relaxação de arestas
- Comparação de desempenho entre algoritmos
- Visualização em tempo real do caminho sendo explorado
- Modo de debug com geração de logs detalhados
- Estatísticas de execução (tempo, nós explorados, caminhos verificados)

## Tecnologias Utilizadas

- **C# (.NET 9.0)** - Linguagem de programação
- **MonoGame** - Framework para desenvolvimento de jogos e aplicações gráficas multiplataforma

## Algoritmos Implementados

### Brute Force
- Explora todos os caminhos possíveis entre o nó inicial e final
- Complexidade: O(n!)
- Adequado apenas para grafos pequenos (até ~10 nós)

### Dijkstra
- Algoritmo clássico de menor caminho
- Utiliza fila de prioridade para explorar nós mais próximos primeiro
- Implementa relaxação de arestas
- Complexidade: O((V + E) log V)
- Eficiente para grafos de qualquer tamanho

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
- Todos os caminhos/nós explorados
- Estatísticas finais

## Uso da Aplicação

1. **Selecionar Nós**: 
   - Clique em um nó para selecioná-lo como ponto inicial (azul)
   - Clique em outro nó para selecioná-lo como ponto final (laranja)
   - Clique novamente para limpar a seleção
   - Se nenhum nó for selecionado, o algoritmo usará nó 0 como inicial e último nó como final

2. **Trocar Grafo**: Alterna entre os grafos

3. **Força Bruta**: Executa o algoritmo de força bruta entre os nós selecionados

4. **Dijkstra**: Executa o algoritmo clássico de Dijkstra entre os nós selecionados

Durante a execução:
- O caminho atual sendo explorado é destacado em tempo real
- Estatísticas são atualizadas continuamente
- A interface mostra:
  - Nós selecionados (inicial e final)
  - Número de caminhos/nós explorados
  - Melhor distância encontrada até o momento
  - Tempo de execução

## Comparação de Performance

