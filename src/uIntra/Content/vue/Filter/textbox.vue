<template>
    <input type="text" :placeholder='placeholder' :value='value'>
</template>

<script>
export default {
    name: 'filter-textbox',
    props:{
        value:String,
        placeholder:String
    },
    methods:{
        
    }
}
</script>
