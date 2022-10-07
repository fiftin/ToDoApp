<template>
    <div class="todo">
        <div>{{ props.todo.text }}</div>
    </div>
</template>

<script setup>
    import { defineProps } from 'vue';

    import Todo from '../models/Todo';

    const props = defineProps({
        todo: {
            type: Todo,
            required: true,
        },
    });
</script>