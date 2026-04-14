<!-- spec-lite managed — regenerated on spec-lite init/update -->

# Project Instructions

This project uses [spec-lite](https://github.com/abranjith/spec-lite) sub-agent prompts
for structured software engineering workflows.

## Available Sub-Agents

The following specialist sub-agents are available:

**Agent files** (`.claude/agents/`):

- [spec.architect](.claude/agents/spec.architect.md)
- [spec.brainstormer](.claude/agents/spec.brainstormer.md)
- [spec.code_reviewer](.claude/agents/spec.code_reviewer.md)
- [spec.data_model_builder](.claude/agents/spec.data_model_builder.md)
- [spec.devops](.claude/agents/spec.devops.md)
- [spec.explorer](.claude/agents/spec.explorer.md)
- [spec.feature](.claude/agents/spec.feature.md)
- [spec.fixer](.claude/agents/spec.fixer.md)
- [spec.implementer](.claude/agents/spec.implementer.md)
- [spec.integration_tester](.claude/agents/spec.integration_tester.md)
- [spec.performance_reviewer](.claude/agents/spec.performance_reviewer.md)
- [spec.feature_planner](.claude/agents/spec.feature_planner.md)
- [spec.planner](.claude/agents/spec.planner.md)
- [spec.security_reviewer](.claude/agents/spec.security_reviewer.md)
- [spec.todo](.claude/agents/spec.todo.md)
- [spec.tool_helper](.claude/agents/spec.tool_helper.md)
- [spec.unit_tester](.claude/agents/spec.unit_tester.md)
- [spec.yolo](.claude/agents/spec.yolo.md)

**Prompt files** (`.claude/prompts/`):

- [spec.architect](.claude/prompts/spec.architect.md)
- [spec.brainstorm](.claude/prompts/spec.brainstorm.md)
- [spec.review_code](.claude/prompts/spec.review_code.md)
- [spec.build_data_model](.claude/prompts/spec.build_data_model.md)
- [spec.devops](.claude/prompts/spec.devops.md)
- [spec.explore](.claude/prompts/spec.explore.md)
- [spec.feature](.claude/prompts/spec.feature.md)
- [spec.fix](.claude/prompts/spec.fix.md)
- [spec.implement](.claude/prompts/spec.implement.md)
- [spec.write_integration_tests](.claude/prompts/spec.write_integration_tests.md)
- [spec.memorize](.claude/prompts/spec.memorize.md)
- [spec.review_performance](.claude/prompts/spec.review_performance.md)
- [spec.plan_feature](.claude/prompts/spec.plan_feature.md)
- [spec.plan](.claude/prompts/spec.plan.md)
- [spec.write_readme](.claude/prompts/spec.write_readme.md)
- [spec.review_security](.claude/prompts/spec.review_security.md)
- [spec.help](.claude/prompts/spec.help.md)
- [spec.todo](.claude/prompts/spec.todo.md)
- [spec.tool_help](.claude/prompts/spec.tool_help.md)
- [spec.write_unit_tests](.claude/prompts/spec.write_unit_tests.md)
- [spec.yolo](.claude/prompts/spec.yolo.md)

## Usage

To use a sub-agent, reference its prompt file in your conversation:

```text
Use the planner from .claude/agents/spec.planner.md to create a technical plan for this project.
```

## Output Directory

Sub-agent outputs are written to the `.spec-lite/` directory:

```text
.spec-lite/
├── brainstorm.md
├── plan.md                    # Default plan (simple projects)
├── plan_<name>.md              # Named plans (complex projects)
├── TODO.md
├── features/
└── reviews/
```
