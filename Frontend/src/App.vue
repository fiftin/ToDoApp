<template>

  <h1>To Do</h1>

  <TodoView 
    :style="{marginLeft: `${(todo.parentPath || '').length}px`}" 
    v-for="todo in todos.items" 
    :todo="todo" 
    :key="todo.id" 
  />

</template>

<script setup lang="ts">
    import {
        plainToInstance,
    } from 'class-transformer';

    import { reactive } from 'vue';

    import Todo from './models/Todo';

    import TodoView from './components/TodoView.vue';

    const todos = reactive({ items: new Array<Todo>() });

    fetch('todos')
        .then(r => r.json())
        .then(json => {
            todos.items = plainToInstance(Todo, json as unknown[]);
        });
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
}
</style>
