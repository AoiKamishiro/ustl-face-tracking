// @ts-check

const {themes} = require('prism-react-renderer');

const siteUrl = process.env.DOCS_URL || 'https://example.com';
const baseUrl = process.env.DOCS_BASE_URL || '/';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'U-Stella FaceTracking',
  tagline: 'VRChatアバター向けフェイシャルトラッキング設定ツール',
  url: siteUrl,
  baseUrl,
  trailingSlash: true,
  onBrokenLinks: 'throw',
  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },
  i18n: {
    defaultLocale: 'ja',
    locales: ['ja'],
  },
  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          routeBasePath: '/',
          sidebarPath: require.resolve('./sidebars.js'),
        },
        blog: false,
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],
  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      navbar: {
        title: 'U-Stella FaceTracking',
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'docs',
            position: 'left',
            label: 'ドキュメント',
          },
          {
            href: 'https://github.com/AoiKamishiro/ustl-face-tracking',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {
                label: '利用方法',
                to: '/usage/',
              },
              {
                label: '機器情報の追加方法',
                to: '/add-hardware-profile/',
              },
            ],
          },
          {
            title: 'Project',
            items: [
              {
                label: 'GitHub',
                href: 'https://github.com/AoiKamishiro/ustl-face-tracking',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} U-Stella Inc. Built with Docusaurus.`,
      },
      prism: {
        theme: themes.github,
        darkTheme: themes.dracula,
      },
    }),
};

module.exports = config;
