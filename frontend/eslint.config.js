// @ts-check

import js from '@eslint/js';
import { defineConfig } from 'eslint/config';
import tseslint from 'typescript-eslint';
import pluginQuery from '@tanstack/eslint-plugin-query'

export default defineConfig(
    js.configs.recommended,
    tseslint.configs.recommended,
    ...pluginQuery.configs['flat/recommended'],

    {
        rules: {
            "prefer-const": "off",
        }
    }
);