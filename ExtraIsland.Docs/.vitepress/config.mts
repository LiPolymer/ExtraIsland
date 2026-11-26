import { defineConfig } from 'vitepress'

const base = '/ExtraIsland/'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  lang: 'zh-CN',
  title: 'ExtraIsland',
  base,
  description: 'ExtraIsland（课岛进化）官方文档与使用指南',
  lastUpdated: true,

  head: [
    ['link', { rel: 'icon', type: 'image/svg+xml', href: `${base}images/extraIsland.svg` }],
    ['link', { rel: 'icon', type: 'image/png', href: `${base}images/extraIsland.png` }],
    ['link', { rel: 'apple-touch-icon', href: `${base}images/extraIsland.png` }]
  ],

  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    logo: '/images/extraIsland.svg',

    nav: [
      { text: '快速开始', link: '/instruction/gettingStarted', activeMatch: '/instruction/' },
      { text: '组件', link: '/components/', activeMatch: '/components/' },
      { text: '自动化', link: '/automations/', activeMatch: '/automations/' },
      {
        text: '更多',
        items: [
          { text: '全局设置', link: '/instruction/settings' },
          { text: '联动生态', link: '/instruction/integrations' },
          { text: 'U 盘访问验证', link: '/security/usb-authorizer' },
          { text: '常见问题', link: '/information/faq' },
          { text: '讨论社区', link: '/information/community' }
        ]
      },
      { text: 'Releases', link: 'https://github.com/LiPolymer/ExtraIsland/releases' }
    ],

    sidebar: [
      {
        text: '开始',
        items: [
          { text: '快速开始', link: '/instruction/gettingStarted' },
          { text: '全局设置', link: '/instruction/settings' },
          { text: '联动生态', link: '/instruction/integrations' }
        ]
      },
      {
        text: '组件',
        items: [
          { text: '组件总览', link: '/components/' },
          { text: '流畅时钟', link: '/components/fluent-clock' },
          { text: '更好的倒计时', link: '/components/better-countdown' },
          { text: '值日表', link: '/components/on-duty' },
          { text: '名句一言', link: '/components/rhesis' },
          { text: '当前活动', link: '/components/live-activity' },
          { text: '动态歌词', link: '/components/dynamic-lyrics' },
          { text: '档案信息', link: '/components/profile-information' },
          { text: '展示标志', link: '/components/flag-display' },
          { text: '双行容器（实验性）', link: '/components/dual-line-container' },
          { text: 'Sleepy（生活模式）', link: '/components/sleepy' }
        ]
      },
      {
        text: '自动化与安全',
        items: [
          { text: '自动化与 SAI 扩展', link: '/automations/' },
          { text: 'U 盘访问验证', link: '/security/usb-authorizer' }
        ]
      },
      {
        text: '信息',
        items: [
          { text: '讨论社区', link: '/information/community' },
          { text: '常见问题与排查', link: '/information/faq' }
        ]
      }
    ],

    search: {
      provider: 'local',
      options: {
        translations: {
          button: {
            buttonText: '搜索文档',
            buttonAriaLabel: '搜索文档'
          },
          modal: {
            noResultsText: '无法找到相关结果',
            resetButtonTitle: '清除查询条件',
            footer: {
              selectText: '选择',
              navigateText: '切换',
              closeText: '关闭'
            }
          }
        }
      }
    },

    outline: {
      level: [2, 3],
      label: '页面导航'
    },

    docFooter: {
      prev: '上一页',
      next: '下一页'
    },

    lastUpdated: {
      text: '最后更新于'
    },

    editLink: {
      pattern: 'https://github.com/LiPolymer/ExtraIsland/edit/v1/ExtraIsland.Docs/:path',
      text: '在 GitHub 上编辑此页'
    },

    returnToTopLabel: '回到顶部',
    sidebarMenuLabel: '菜单',
    darkModeSwitchLabel: '主题',
    lightModeSwitchTitle: '切换到浅色模式',
    darkModeSwitchTitle: '切换到深色模式',

    socialLinks: [
      { icon: 'github', link: 'https://github.com/LiPolymer/ExtraIsland', ariaLabel: 'GitHub' },
      {
        icon: {
          svg: '<svg role="img" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><title>GitLab</title><path d="m23.6004 9.5927-.0337-.0862L20.3.9814a.851.851 0 0 0-.3362-.405.8748.8748 0 0 0-.9997.0539.8748.8748 0 0 0-.29.4399l-2.2055 6.748H7.5375l-2.2057-6.748a.8573.8573 0 0 0-.29-.4412.8748.8748 0 0 0-.9997-.0537.8585.8585 0 0 0-.3362.4049L.4332 9.5015l-.0325.0862a6.0657 6.0657 0 0 0 2.0119 7.0105l.0113.0087.03.0213 4.976 3.7264 2.462 1.8633 1.4995 1.1321a1.0085 1.0085 0 0 0 1.2197 0l1.4995-1.1321 2.4619-1.8633 5.006-3.7489.0125-.01a6.0682 6.0682 0 0 0 2.0094-7.003z"/></svg>'
        },
        link: 'https://gitlab.com/LiPolymer/ExtraIsland',
        ariaLabel: 'GitLab'
      }
    ],

    footer: {
      message: '全站内容及源代码均以 <a href="https://www.gnu.org/licenses/agpl-3.0.html">GNU AGPLv3</a> 协议许可',
      copyright: '版权所有 © 2024-2026 LiPolymer & all Contributors'
    }
  }
})
