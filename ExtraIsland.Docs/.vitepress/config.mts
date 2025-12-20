import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "ExtraIsland",
  base: "/ExtraIsland/",
  description: "Documents & Instructions",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: '文档', link: '/' },
      { text: 'GitLab', link: 'https://gitlab.com/LiPolymer/ExtraIsland/' }
    ],
    
    footer: {
      message: '全站内容及源代码均以<a href="https://www.gnu.org/licenses/agpl-3.0.html">GNU AGPLv3</a>协议许可',
      copyright: '版权所有 © 2025 LiPolymer & all Contributors'
    },
    
    sidebar: [
      {
        text: '开始',
        items: [
          { text: '快速开始', link: '/instruction/gettingStarted' }
        ]
      }, 
      {
          text: '信息',
          items: [
              { text: '讨论社区', link: '/information/community' }
          ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/LiPolymer/ExtraIsland' }
    ],

    logo: '/images/extraIsland.svg'
  }
})