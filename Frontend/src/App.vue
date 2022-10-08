<template>

    <h1>To Do App</h1>

    <div style="margin-bottom: 20px;">
        <button @click="newTodo = true;" v-if="!newTodo">New To Do</button>

        <div v-if="newTodo">
            <input type="text" v-model="newTodoText" />
            <button style="margin-left: 5px;" @click="addNewTodo()">Add</button>
            <button style="margin-left: 5px;" @click="newTodo = false;">Cancel</button>
        </div>
    </div>

    <TodoView :style="{marginLeft: `${(todo.parentPath || '').length}px`}"
              v-for="todo in todos.items"
              :todo="todo"
              :key="todo.id"
              @changed="loadTodos()" />
</template>

<script setup lang="ts">
    import { plainToInstance } from 'class-transformer';
    import { reactive, ref } from 'vue';
    import Todo from './models/Todo';
    import TodoView from './components/TodoView.vue';

    const todos = reactive({ items: new Array<Todo>() });
    const newTodo = ref(false);
    const newTodoText = ref('');

    async function addNewTodo() {
        await fetch(`todos`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                text: newTodoText.value
            }),
        });

        newTodo.value = false;
        newTodoText.value = '';
        await loadTodos();
    }

    async function loadTodos() {
        const json = await fetch('todos').then(r => r.json());
        todos.items = plainToInstance(Todo, json as unknown[]);
    }

    loadTodos();
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
}
</style>
