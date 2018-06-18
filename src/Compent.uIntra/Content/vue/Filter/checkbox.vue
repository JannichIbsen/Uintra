<template>
    <div class="check">
        <label class="check__label">
            <input type="checkbox" class="check__box" v-model="val"
                @change="$emit('change')">
            <span class="check__fake-input icon-check"></span>
            <span class="check__fake-label">{{label}}</span>
        </label>
    </div>
</template>
 
<script>
export default {
    name: 'filter-checkbox',
    props:{
        value: Object
    },
    computed:{
        val: {
            get: function(){
                return this.value.checked;
            },
            set: function(v){
                this.value.checked = v;
            }
        },
        label: function(){
            return this.value.label || 'default label';
        }
    },
    methods:{
        clearData(){
            this.value.checked = false;
        }
    }
}
</script>