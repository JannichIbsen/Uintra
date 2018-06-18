<template>
    <div class="filter-form" :class="{ 'filter-form--opened': showContent }">
        <h2 class="filter-form__opener" @click='toggleContent' :class="{ 'filter-form__opener--active': showContent }">Filter</h2>
        <div class="filter-form__content" v-show="showContent">
            <div class="sub-filters">
                <div v-for="(filter, index) in filterGroups"
                    :is='filter.type'
                    :value='filter.data'
                    :key="index"
                    @change="stateChange(filter)">
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import Vue from "vue";
    import feedStateService from './../../../App_Plugins/CentralFeed/feedStateService';
    import checkbox from './checkbox';

    export default {
        name: "custom-filter",
        components: {
            checkbox
        },
        data() {
            return {
                showContent: false, 
                filterGroups: [
                    {
                        type: "checkbox",
                        data: {
                            checked:true
                        },
                        stateKey: "isChecked"
                    }
                ]
            }
        },
        methods: {
            toggleContent() {
                this.showContent = !this.showContent;
            },
            stateChange(filter){
                feedStateService.changeState(filter.stateKey, filter.data.checked);
            }
        }
    };
</script>
