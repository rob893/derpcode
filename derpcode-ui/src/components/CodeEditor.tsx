import Editor from '@monaco-editor/react';
import { Language } from '../types/models';

interface CodeEditorProps {
  language: Language;
  code: string;
  uiTemplate: string;
  onChange: (value: string | undefined) => void;
}

export const CodeEditor = ({ language, code, onChange, uiTemplate }: CodeEditorProps) => {
  const getMonacoLanguage = (lang: Language) => {
    switch (lang) {
      case Language.CSharp:
        return 'csharp';
      case Language.JavaScript:
        return 'javascript';
      case Language.TypeScript:
        return 'typescript';
      default:
        return 'javascript';
    }
  };

  return (
    <div className="code-editor">
      <Editor
        height="70vh"
        language={getMonacoLanguage(language)}
        value={code || uiTemplate}
        onChange={onChange}
        theme="vs-dark"
        options={{
          minimap: { enabled: false },
          fontSize: 14,
          lineNumbers: 'on',
          roundedSelection: false,
          scrollBeyondLastLine: false,
          automaticLayout: true
        }}
      />
    </div>
  );
};
