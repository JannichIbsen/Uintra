<template>
    <div class="filter-form">
      <div v-for="item in filterGroups">
        
      </div>
    </div>
</template>

<script>
import checkbox from './checkbox.vue';
import textbox from './textbox.vue';

export default {
  name: "custom-filter",
  components:[checkbox, textbox],
  data() {
    return {
      //example model
      filterGroups:[
        {
          title: 'News',
          state: '...',
          subFilters:[
            {
              type:'checkbox',
              value:Boolean
            },
            {
              type:'text',
              value: 'test text'
            }
          ]
        },
        {
          title:'Events',
          state:'...',
          subFilters:[
            {
              type:'checkbox',
              value:Boolean
            }
          ]
        }

      ]
    };
  },
  created: function() {},
  mounted() {},
  computed: {},
  methods: {}
};
</script>

