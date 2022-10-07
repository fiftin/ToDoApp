<template>
    <div class="todo">
        <div v-if="editTodo">
            <input type="text" v-model="editTodoText" />
            <button style="margin-left: 5px;" @click="setText()">Save</button>
            <button style="margin-left: 5px;" @click="editTodo = false;">Cancel</button>
        </div>
        <div v-else>
            <input type="checkbox" v-model="isDone" />
            <span :style="{textDecoration: props.todo.isDone ? 'line-through' : ''}"
                  style="margin-right: 10px;">{{ props.todo.text }}</span>
            <button style="margin-left: 5px;" @click="editTodo = true;">Edit</button>
            <button style="margin-left: 5px;" @click="remove();">Delete</button>
            <button style="margin-left: 5px;" @click="newTodo = true;" v-if="!newTodo">Add</button>
        </div>
        <div style="margin-top: 10px; margin-left: 20px;" v-if="newTodo">
            <input type="text" v-model="newTodoText" />
            <button style="margin-left: 5px;" @click="addNewTodo()">Add</button>
            <button style="margin-left: 5px;" @click="newTodo = false;">Cancel</button>
        </div>
    </div>
</template>

<style>
    .todo {
        margin-bottom: 10px;
    }
</style>

<script setup>
    import { defineProps, defineEmits, computed, ref } from 'vue';

    import Todo from '../models/Todo';

    const isDone = computed({
        get() {
            return props.todo.isDone;
        },
        set(newValue) {
            setIsDone(newValue);
        },
    });

    const props = defineProps({
        todo: {
            type: Todo,
            required: true,
        },
    });

    const emit = defineEmits(['changed']);

    const editTodo = ref(false);
    const editTodoText = ref('');

    const newTodo = ref(false);
    const newTodoText = ref('');

    async function addNewTodo() {

        await fetch(`todos`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                text: newTodoText.value,
                parentPath: props.todo.parentPath ? `${props.todo.parentPath}/${props.todo.id}` : props.todo.id,
            }),
        });

        newTodo.value = false;
        newTodoText.value = '';
         

        emit('changed');
    }

    async function setIsDone(newValue) {
        await fetch(`todos/${props.todo.id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ isDone: newValue }),
        });

        emit('changed');
    }


    async function setText() {
        await fetch(`todos/${props.todo.id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ text: editTodoText.value }),
        });

        editTodo.value = false;
        editTodoText.value = '';

        emit('changed');
    }

    async function remove() {
        await fetch(`todos/${props.todo.id}`, { method: 'DELETE' });
        emit('changed');
    }
</script>